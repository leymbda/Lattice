namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
//open Microsoft.DurableTask.Client
open Microsoft.DurableTask.Entities
open System
open System.Threading.Tasks

type ShardEvent =
    | GET
    | CREATE of startAt: DateTime
    | TRANSFER of transferAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime

type ShardCreateInput = {
    ShardId: ShardId
    StartAt: DateTime
}

type ShardTransferInput = {
    ShardId: ShardId
    TransferAt: DateTime
}

type ShardShutdownInput = {
    ShardId: ShardId
    ShutdownAt: DateTime
}


module ShardEntity =
    let [<Literal>] entityName = "ShardEntity"
    let [<Literal>] createOrchestratorName = "ShardCreateOrchestrator"
    let [<Literal>] transferOrchestratorName = "ShardTransferOrchestrator"
    let [<Literal>] shutdownOrchestratorName = "ShardShutdownOrchestrator"

    let entityId (shardId: ShardId) =
        EntityInstanceId(entityName, ShardId.toString shardId)

    //let send shardId event (client: DurableTaskClient) =
    //    match event with
    //    | ShardEvent.CREATE startAt ->
    //        client.Entities.SignalEntityAsync(entityId shardId, nameof ShardEvent.CREATE, startAt)

    //    | ShardEvent.TRANSFER transferAt ->
    //        client.Entities.SignalEntityAsync(entityId shardId, nameof ShardEvent.TRANSFER, transferAt)

    //    | ShardEvent.SHUTDOWN shutdownAt ->
    //        client.Entities.SignalEntityAsync(entityId shardId, nameof ShardEvent.SHUTDOWN, shutdownAt)

    //let getState shardId (client: DurableTaskClient) =
    //    client.Entities.GetEntityAsync<Shard>(entityId shardId, true)
    //    |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type ShardEntity (env: IEnv) =
    [<Function(ShardEntity.createOrchestratorName)>]
    member _.Create (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: ShardCreateInput
    ) = task {
        // - Pick available node
        // - Create instance
        // - Await suborchestrator completion?

        // TODO: Implement

        return ()
    }

    [<Function(ShardEntity.transferOrchestratorName)>]
    member _.Transfer (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: ShardTransferInput
    ) = task {
        // Get current entity state
        let! shard = ctx.Entities.CallEntityAsync<Shard>(ShardEntity.entityId input.ShardId, nameof ShardEvent.GET)

        // Shut down currently active or soon to be active instance
        let currentNode =
            match Shard.getState ctx.CurrentUtcDateTime shard with
            | ShardState.Starting (next, _) -> Some next
            | ShardState.Active current -> Some current
            | ShardState.Transferring (_, next, _) -> Some next
            | _ -> None

        match currentNode with
        | None -> ()
        | Some currentNode ->
            do! ctx.Entities.CallEntityAsync(
                ShardInstanceEntity.entityId shard.Id currentNode,
                nameof ShardInstanceEvent.SHUTDOWN,
                input.TransferAt)

        // Create new instance
        let createInput: ShardCreateInput = {
            ShardId = shard.Id
            StartAt = input.TransferAt
        }

        return! ctx.CallSubOrchestratorAsync(ShardEntity.createOrchestratorName, createInput)
    }

    [<Function(ShardEntity.shutdownOrchestratorName)>]
    member _.Shutdown (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: ShardShutdownInput
    ) = task {
        // Get current entity state
        let! shard = ctx.Entities.CallEntityAsync<Shard>(ShardEntity.entityId input.ShardId, nameof ShardEvent.GET)

        // Notify all active or soon to be active instances of shutdown
        let nodes =
            match Shard.getState ctx.CurrentUtcDateTime shard with
            | ShardState.Starting (next, _) -> [next]
            | ShardState.Active current -> [current]
            | ShardState.Transferring (current, next, _) -> [current; next]
            | _ -> []

        return!
            nodes
            |> List.map (fun id -> ctx.Entities.CallEntityAsync(
                ShardInstanceEntity.entityId shard.Id id,
                nameof ShardInstanceEvent.SHUTDOWN,
                input.ShutdownAt))
            |> Task.WhenAll
    }

    [<Function(ShardEntity.entityName)>]
    member _.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let shardId =
                    ShardId.fromString op.Context.Id.Key
                    |> Option.defaultWith (failwith $"Invalid shard ID: {op.Context.Id.Key}")

                let state = op.State.GetState<Shard>(Shard.create shardId)

                match op.Name with
                | nameof ShardEvent.CREATE ->
                    let input: ShardCreateInput = {
                        ShardId = state.Id
                        StartAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardEntity.createOrchestratorName, input) |> ignore
                    return state
                    
                | nameof ShardEvent.TRANSFER ->
                    let input: ShardTransferInput = {
                        ShardId = state.Id
                        TransferAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardEntity.transferOrchestratorName, input) |> ignore
                    return state

                | nameof ShardEvent.SHUTDOWN ->
                    let input: ShardShutdownInput = {
                        ShardId = state.Id
                        ShutdownAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardEntity.shutdownOrchestratorName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map (fun s -> op.State.SetState s; s)
            |> ValueTask<obj>
        )
        
    // TODO: Handle deleting entity by setting state to null (how to handle async through shutdown orchestrations?)
