namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open Microsoft.DurableTask.Client
open System
open System.Threading.Tasks

module ShardEntity =
    let send shardId event (client: DurableTaskClient) =
        match event with
        | ShardEvent.CREATE startAt ->
            client.Entities.SignalEntityAsync(ShardEvent.entityId shardId, nameof ShardEvent.CREATE, startAt)

        | ShardEvent.TRANSFER transferAt ->
            client.Entities.SignalEntityAsync(ShardEvent.entityId shardId, nameof ShardEvent.TRANSFER, transferAt)

        | ShardEvent.SHUTDOWN shutdownAt ->
            client.Entities.SignalEntityAsync(ShardEvent.entityId shardId, nameof ShardEvent.SHUTDOWN, shutdownAt)

    let getState shardId (client: DurableTaskClient) =
        client.Entities.GetEntityAsync<Shard>(ShardEvent.entityId shardId, true)
        |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type ShardEntity (env: IEnv) =
    [<Function(ShardEvent.orchestratorCreateName)>]
    member _.Create (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        input: ShardCreateInput
    ) = task {
        // - Pick available node
        // - Create instance
        // - Await suborchestrator completion?

        // TODO: Implement

        return ()
    }

    [<Function(ShardEvent.orchestratorTransferName)>]
    member _.Transfer (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        input: ShardTransferInput
    ) = task {
        // Shut down currently active or soon to be active instance
        let currentNode =
            match Shard.getState ctx.CurrentUtcDateTime input.Shard with
            | ShardState.Starting (next, _) -> Some next
            | ShardState.Active current -> Some current
            | ShardState.Transferring (_, next, _) -> Some next
            | _ -> None

        match currentNode with
        | None -> ()
        | Some currentNode ->
            do! ctx.Entities.CallEntityAsync(
                ShardInstanceEvent.entityId input.Shard.Id currentNode,
                nameof ShardInstanceEvent.SHUTDOWN,
                input.TransferAt)

        // Create new instance
        let createInput: ShardCreateInput = {
            Shard = input.Shard
            StartAt = input.TransferAt
        }

        return! ctx.CallSubOrchestratorAsync(ShardEvent.orchestratorCreateName, input)
    }

    [<Function(ShardEvent.orchestratorShutdownName)>]
    member _.Shutdown (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: ShardShutdownInput
    ) =
        // Notify all active or soon to be active instances of shutdown
        let nodes =
            match Shard.getState ctx.CurrentUtcDateTime input.Shard with
            | ShardState.Starting (next, _) -> [next]
            | ShardState.Active current -> [current]
            | ShardState.Transferring (current, next, _) -> [current; next]
            | _ -> []

        nodes
        |> List.map (fun id -> ctx.Entities.CallEntityAsync(
            ShardInstanceEvent.entityId input.Shard.Id id,
            nameof ShardInstanceEvent.SHUTDOWN,
            input.ShutdownAt))
        |> Task.WhenAll

    [<Function(ShardEvent.entityName)>]
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
                        Shard = state
                        StartAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardEvent.orchestratorCreateName, input) |> ignore
                    return state
                    
                | nameof ShardEvent.TRANSFER ->
                    let input: ShardTransferInput = {
                        Shard = state
                        TransferAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardEvent.orchestratorCreateName, input) |> ignore
                    return state

                | nameof ShardEvent.SHUTDOWN ->
                    let input: ShardShutdownInput = {
                        Shard = state
                        ShutdownAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(ShardEvent.orchestratorShutdownName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map op.State.SetState // TODO: Figure out how to handle race conditions
            |> ValueTask<obj>
        )
