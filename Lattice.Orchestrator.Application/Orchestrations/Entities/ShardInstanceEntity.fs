namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
//open Microsoft.DurableTask.Client
open Microsoft.DurableTask.Entities
open System
open System.Threading.Tasks

type ShardInstanceEvent =
    | GET
    | CREATE of startAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime

type ShardInstanceCreateInput = {
    NodeId: Guid
    ShardId: ShardId
    StartAt: DateTime
}

type ShardInstanceShutdownInput = {
    NodeId: Guid
    ShardId: ShardId
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
                | nameof ShardInstanceEvent.GET ->
                    return state

                | nameof ShardInstanceEvent.CREATE ->
                    let input: ShardInstanceCreateInput = {
                        NodeId = state.NodeId
                        ShardId = state.ShardId
                        StartAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardInstanceEntity.createOrchestratorName, input) |> ignore
                    return state

                | nameof ShardInstanceEvent.SHUTDOWN ->
                    let input: ShardInstanceShutdownInput = {
                        NodeId = state.NodeId
                        ShardId = state.ShardId
                        ShutdownAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardInstanceEntity.shutdownOrchestratorName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map (fun s -> op.State.SetState s; s)
            |> ValueTask<obj>
        )
        
    // TODO: Handle deleting entity by setting state to null (how to handle async through shutdown orchestrations?)
