namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
//open Microsoft.DurableTask.Client
open Microsoft.DurableTask.Entities
open System
open System.Threading.Tasks

type ShardInstanceEvent =
    | CREATE of startAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime

type ShardInstanceCreateInput = {
    ShardInstance: ShardInstance
    StartAt: DateTime
}

type ShardInstanceShutdownInput = {
    ShardInstance: ShardInstance
    ShutdownAt: DateTime
}

module ShardInstanceEntity =
    let [<Literal>] ENTITY_ID_SEPARATOR = "|"

    let [<Literal>] entityName = "ShardInstanceEntity"
    let [<Literal>] createOrchestratorName = "ShardInstanceCreateOrchestrator"
    let [<Literal>] shutdownOrchestratorName = "ShardInstanceShutdownOrchestrator"

    let parseEntityId (id: EntityInstanceId) =
        match id.Name, id.Key.Split ENTITY_ID_SEPARATOR with
        | name, [| shardId; nodeId |] when name = entityName ->
            match Guid.TryParse nodeId, ShardId.fromString shardId with
            | (true, nodeId), Some shardId -> Some (shardId, nodeId)
            | _ -> None

        | _ -> None

    let entityId (shardId: ShardId) (nodeId: Guid) =
        EntityInstanceId(entityName, nodeId.ToString() + ENTITY_ID_SEPARATOR + ShardId.toString shardId)
        
    //let send shardId nodeId event (client: DurableTaskClient) =
    //    match event with
    //    | ShardInstanceEvent.CREATE startAt ->
    //        client.Entities.SignalEntityAsync(entityId shardId nodeId, nameof ShardInstanceEvent.CREATE, startAt)

    //    | ShardInstanceEvent.SHUTDOWN shutdownAt ->
    //        client.Entities.SignalEntityAsync(entityId shardId nodeId, nameof ShardInstanceEvent.SHUTDOWN, shutdownAt)

    //let getState shardId nodeId (client: DurableTaskClient) =
    //    client.Entities.GetEntityAsync<ShardInstance>(entityId shardId nodeId, true)
    //    |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type ShardInstanceEntity (env: IEnv) =
    [<Function(ShardInstanceEntity.createOrchestratorName)>]
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
    
    [<Function(ShardInstanceEntity.shutdownOrchestratorName)>]
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

    [<Function(ShardInstanceEntity.entityName)>]
    member this.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let shardId, nodeId =
                    ShardInstanceEntity.parseEntityId op.Context.Id
                    |> Option.defaultWith (failwith $"Invalid shard instance ID: {op.Context.Id.Key}")

                let state = op.State.GetState<ShardInstance>(ShardInstance.create shardId nodeId)

                match op.Name with
                | nameof ShardInstanceEvent.CREATE ->
                    let input: ShardInstanceCreateInput = {
                        ShardInstance = state
                        StartAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardInstanceEntity.createOrchestratorName, input) |> ignore
                    return state

                | nameof ShardInstanceEvent.SHUTDOWN ->
                    let input: ShardInstanceShutdownInput = {
                        ShardInstance = state
                        ShutdownAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardInstanceEntity.shutdownOrchestratorName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map op.State.SetState // TODO: Figure out how to handle race conditions
            |> ValueTask<obj>
        )
