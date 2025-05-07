namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open System
open System.Threading.Tasks
open Microsoft.DurableTask.Client

module ShardInstanceEntity =
    let send shardId nodeId event (client: DurableTaskClient) =
        match event with
        | ShardInstanceEvent.START startAt ->
            client.Entities.SignalEntityAsync(ShardInstanceEvent.entityId shardId nodeId, nameof ShardInstanceEvent.START, startAt)

        | ShardInstanceEvent.SHUTDOWN shutdownAt ->
            client.Entities.SignalEntityAsync(ShardInstanceEvent.entityId shardId nodeId, nameof ShardInstanceEvent.SHUTDOWN, shutdownAt)

    let getState shardId nodeId (client: DurableTaskClient) =
        client.Entities.GetEntityAsync<ShardInstance>(ShardInstanceEvent.entityId shardId nodeId, true)
        |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type ShardInstanceEntity (env: IEnv) =
    member _.Start startAt shardInstance = task {
        return shardInstance
        // TODO: Notify node to start instance at given time
    }
    
    member _.Shutdown shutdownAt shardInstance = task {
        return shardInstance
        // TODO: Notify node to stop instance at given time
    }

    [<Function(ShardInstanceEvent.entityName)>]
    member this.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let shardId, nodeId =
                    ShardInstanceEvent.parseEntityId op.Context.Id
                    |> Option.defaultWith (failwith $"Invalid shard instance ID: {op.Context.Id.Key}")

                let state = op.State.GetState<ShardInstance>(ShardInstance.create shardId nodeId)

                match op.Name with
                | nameof ShardInstanceEvent.START ->
                    let startAt = op.GetInput<DateTime>()
                    return! this.Start startAt state

                | nameof ShardInstanceEvent.SHUTDOWN ->
                    let shutdownAt = op.GetInput<DateTime>()
                    return! this.Shutdown shutdownAt state

                | _ -> return state
            }
            |> Task.map op.State.SetState
            |> ValueTask<obj>
        )
