namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open System
open System.Threading.Tasks
open Microsoft.DurableTask.Client

module NodeEntity =
    let send id event (client: DurableTaskClient) =
        match event with
        | NodeEvent.CONNECT ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.CONNECT)

        | NodeEvent.DISCONNECT ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.DISCONNECT)

        | NodeEvent.HEARTBEAT shardIds ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.HEARTBEAT, shardIds)

        | NodeEvent.TRANSFER_ALL_INSTANCES transferAt ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.TRANSFER_ALL_INSTANCES, transferAt)

    let getState id (client: DurableTaskClient) =
        client.Entities.GetEntityAsync<Node>(NodeEvent.entityId id, true)
        |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type NodeEntity (env: IEnv) =
    member _.Connect node = task {
        return node
        // TODO: Register node
    }
    
    member _.Disconnect node = task {
        return node
        // TODO: Immediately transfer all shards if any present and deregister node
    }

    member _.Heartbeat shardIds node = task {
        return node
        // TODO: Validate shard IDs match and handle possible disputes
    }

    member _.TransferAllInstances transferAt node = task {
        return node
        // TODO: Gracefully transfer all shards at the given time
    }

    [<Function(NodeEvent.entityName)>]
    member this.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let id = Guid.Parse op.Context.Id.Key
                let state = op.State.GetState<Node>(Node.create id DateTime.UtcNow)

                match op.Name with
                | nameof NodeEvent.CONNECT ->
                    return! this.Connect state

                | nameof NodeEvent.DISCONNECT ->
                    return! this.Disconnect state

                | nameof NodeEvent.HEARTBEAT ->
                    let shardIds = op.GetInput<ShardId list>()
                    return! this.Heartbeat shardIds state

                | nameof NodeEvent.TRANSFER_ALL_INSTANCES ->
                    let transferAt = op.GetInput<DateTime>()
                    return! this.TransferAllInstances transferAt state

                | _ -> return state
            }
            |> Task.map op.State.SetState
            |> ValueTask<obj>
        )
