namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask.Entities
open System
open System.Threading.Tasks

/// A durable entity to represent a node in the node pool.
type NodeHealthDurableEntity (env: IEnv) =
    inherit TaskEntity<NodeHealth> ()

    [<Function(nameof NodeHealthDurableEntity)>]
    static member Run ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync<NodeHealthDurableEntity>()

    override _.InitializeState operation =
        NodeHealth.create (Guid operation.Context.Id.Key) DateTime.UtcNow

    /// Check if the node has unexpectedly died, and if so, notify orchestrator to release.
    member this.DeleteIfExpired () =
        let id = this.State.Id
        
        match NodeHealth.isAlive DateTime.UtcNow this.State with
        | false -> task { do! env.NodeRelease id }
        | true -> Task.FromResult ()

    /// Handle a heartbeat that signifies the node is still alive.
    member this.Heartbeat heartbeatTime =
        this.State <- NodeHealth.heartbeat heartbeatTime this.State

        this.Context.SignalEntity(
            this.Context.Id,
            nameof this.DeleteIfExpired,
            options = SignalEntityOptions(SignalTime = heartbeatTime.AddSeconds(NodeHealth.LIFETIME_SECONDS)))

    /// Release shards from the node immediately to restore dead shards.
    member this.Release () =
        // TODO: Consider any additional behaviour needed here
        // TODO: Figure out how to disable any event processing from old (current) node in case it tries to restore itself
        this.Context.SignalEntity(this.Context.Id, "delete")

    /// Initiate redistribution of the node's shards to other nodes to ensure no downtime.
    member this.Redistribute () = 
        // TODO: Consider any additional behaviour needed here
        this.State <- NodeHealth.initiateRedistribution DateTime.UtcNow this.State
        