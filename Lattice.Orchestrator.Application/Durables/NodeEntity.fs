namespace Lattice.Orchestrator.Application

open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask.Entities
open System
open System.Threading.Tasks

type NodeRedistributionState = {
    ScheduledCutoff: DateTime
    TransferReady: bool
}

type NodeEntityState = {
    Id: Guid
    LastHeartbeatAck: DateTime
    RedistributionState: NodeRedistributionState option
}

module NodeEntityState =
    let [<Literal>] LIFETIME_SECONDS = Events.NODE_HEARTBEAT_FREQUENCY_SECS * 2
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
        NodeEntityState.create (Guid operation.Context.Id.Key) DateTime.UtcNow

    /// Check if the node has unexpectedly died, and if so, notify orchestrator to release.
    member this.DeleteIfExpired () =
        let id = this.State.Id
        
        match NodeEntityState.isAlive DateTime.UtcNow this.State with
        | false -> task { do! env.Release id }
        | true -> Task.FromResult ()

    /// Handle a heartbeat that signifies the node is still alive.
    member this.Heartbeat heartbeatTime =
        this.State <- NodeEntityState.heartbeat heartbeatTime this.State

        this.Context.SignalEntity(
            this.Context.Id,
            nameof this.DeleteIfExpired,
            options = SignalEntityOptions(SignalTime = heartbeatTime.AddSeconds(NodeEntityState.LIFETIME_SECONDS)))

    /// Release shards from the node immediately to restore dead shards.
    member this.Release () =
        // TODO: Consider any additional behaviour needed here
        // TODO: Figure out how to disable any event processing from old (current) node in case it tries to restore itself
        this.Context.SignalEntity(this.Context.Id, "delete")

    /// Initiate redistribution of the node's shards to other nodes to ensure no downtime.
    member this.Redistribute () = 
        // TODO: Consider any additional behaviour needed here
        this.State <- NodeEntityState.initiateRedistribution DateTime.UtcNow this.State
        