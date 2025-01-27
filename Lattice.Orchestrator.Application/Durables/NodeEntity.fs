namespace Lattice.Orchestrator.Application

open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask.Entities
open System

type NodeRedistributionState = {
    ScheduledCutoff: DateTime
    TransferReady: bool
}

type NodeEntityState = {
    Id: string
    LastHeartbeatAck: DateTime
    RedistributionState: NodeRedistributionState option
}

module NodeEntityState =
    let [<Literal>] LIFETIME_SECONDS = 60
    let [<Literal>] REDISTRIBUTION_CUTOFF_SECONDS = 60

    let create id currentTime = {
        Id = id
        LastHeartbeatAck = currentTime
        RedistributionState = None
    }

    let isAlive currentTime node =
        (currentTime - node.LastHeartbeatAck).TotalSeconds < float LIFETIME_SECONDS

    let heartbeat currentTime node =
        { node with LastHeartbeatAck = currentTime }

    let initiateRedistribution (currentTime: DateTime) node =
        let state = {
            ScheduledCutoff = currentTime.AddSeconds REDISTRIBUTION_CUTOFF_SECONDS
            TransferReady = false
        }

        { node with RedistributionState = Some state }

/// A durable entity to represent a node in the node pool.
type NodeEntity (env: IEnv) =
    inherit TaskEntity<NodeEntityState> ()

    [<Function(nameof NodeEntity)>]
    static member Run ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync<NodeEntity>()

    override _.InitializeState operation =
        NodeEntityState.create operation.Context.Id.Key DateTime.UtcNow

    /// Check if the node has unexpectedly died, and if so, delete it.
    member this.DeleteIfExpired () =
        if not <| NodeEntityState.isAlive DateTime.UtcNow this.State then
            this.Context.SignalEntity(this.Context.Id, "delete")

    /// Handle a heartbeat that signifies the node is still alive.
    member this.Heartbeat () =
        this.State <- NodeEntityState.heartbeat DateTime.UtcNow this.State

        this.Context.SignalEntity(
            this.Context.Id,
            nameof this.DeleteIfExpired,
            options = SignalEntityOptions(SignalTime = DateTime.UtcNow.AddSeconds(NodeEntityState.LIFETIME_SECONDS)))

    /// Release shards from the node immediately to restore dead shards.
    member this.Release () =
        // TODO: Notify orchestrator that the shards are being released immediately
        // TODO: Figure out how to disable any event processing from old (current) node in case it tries to restore itself
        this.Context.SignalEntity(this.Context.Id, "delete")

    /// Initiate redistribution of the node's shards to other nodes to ensure no downtime.
    member this.Redistribute () = 
        // TODO: Trigger redistribution of this node's shards to other nodes
        this.State <- NodeEntityState.initiateRedistribution DateTime.UtcNow this.State
        