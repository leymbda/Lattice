namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open Microsoft.DurableTask.Client
open System
open System.Threading.Tasks

module ShardInstanceEntity =
    let send shardId nodeId event (client: DurableTaskClient) =
        match event with
        | ShardInstanceEvent.CREATE startAt ->
            client.Entities.SignalEntityAsync(ShardInstanceEvent.entityId shardId nodeId, nameof ShardInstanceEvent.CREATE, startAt)

        | ShardInstanceEvent.SHUTDOWN shutdownAt ->
            client.Entities.SignalEntityAsync(ShardInstanceEvent.entityId shardId nodeId, nameof ShardInstanceEvent.SHUTDOWN, shutdownAt)

    let getState shardId nodeId (client: DurableTaskClient) =
        client.Entities.GetEntityAsync<ShardInstance>(ShardInstanceEvent.entityId shardId nodeId, true)
        |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type ShardInstanceEntity (env: IEnv) =
    [<Function(ShardInstanceEvent.orchestratorCreateName)>]
    member _.Create (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        input: ShardInstanceCreateInput
    ) = task {
        // - Await startup
        // - Add disabled reason if failure (?)
        // - Notify shard create of success (?)

        // TODO: Implement

        return ()
    }
    
    [<Function(ShardInstanceEvent.orchestratorShutdownName)>]
    member _.Shutdown (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        input: ShardInstanceShutdownInput
    ) = task {
        // - Await shutdown
        // - Nodify shard shutdown (?)

        // TODO: Implement

        return ()
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
                | nameof ShardInstanceEvent.CREATE ->
                    let input: ShardInstanceCreateInput = {
                        ShardInstance = state
                        StartAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardInstanceEvent.orchestratorCreateName, input) |> ignore
                    return state

                | nameof ShardInstanceEvent.SHUTDOWN ->
                    let input: ShardInstanceShutdownInput = {
                        ShardInstance = state
                        ShutdownAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardInstanceEvent.orchestratorShutdownName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map op.State.SetState // TODO: Figure out how to handle race conditions
            |> ValueTask<obj>
        )
