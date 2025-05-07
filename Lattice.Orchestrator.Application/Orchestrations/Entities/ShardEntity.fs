namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open System
open System.Threading.Tasks
open Microsoft.DurableTask.Client

module ShardEntity =
    let send shardId event (client: DurableTaskClient) =
        match event with
        | ShardEvent.CREATE_OR_TRANSFER transferAt ->
            client.Entities.SignalEntityAsync(ShardEvent.entityId shardId, nameof ShardEvent.CREATE_OR_TRANSFER, transferAt)

        | ShardEvent.SHUTDOWN shutdownAt ->
            client.Entities.SignalEntityAsync(ShardEvent.entityId shardId, nameof ShardEvent.SHUTDOWN, shutdownAt)

    let getState shardId (client: DurableTaskClient) =
        client.Entities.GetEntityAsync<Shard>(ShardEvent.entityId shardId, true)
        |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type ShardEntity (env: IEnv) =
    member _.CreateOrTransfer transferAt shard = task {
        return shard
        // TODO: Initiate shard instance start/transfer for given time
    }
    
    member _.Shutdown shutdownAt shard = task {
        return shard
        // TODO: Shutdown active shard instances at given time
    }

    [<Function(ShardEvent.entityName)>]
    member this.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let shardId =
                    ShardId.fromString op.Context.Id.Key
                    |> Option.defaultWith (failwith $"Invalid shard ID: {op.Context.Id.Key}")

                let state = op.State.GetState<Shard>(Shard.create shardId)

                match op.Name with
                | nameof ShardEvent.CREATE_OR_TRANSFER ->
                    let transferAt = op.GetInput<DateTime>()
                    return! this.CreateOrTransfer transferAt state

                | nameof ShardEvent.SHUTDOWN ->
                    let shutdownAt = op.GetInput<DateTime>()
                    return! this.Shutdown shutdownAt state

                | _ -> return state
            }
            |> Task.map op.State.SetState
            |> ValueTask<obj>
        )
