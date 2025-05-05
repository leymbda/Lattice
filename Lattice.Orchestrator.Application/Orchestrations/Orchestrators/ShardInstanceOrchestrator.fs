namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open System

type ShardInstanceOrchestrator () =
    [<Function(ShardInstanceEvent.orchestratorName)>]
    static member Run (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        shardInstance: ShardInstance
    ) = task {
        let ct = fctx.CancellationToken
        let currentTime = ctx.CurrentUtcDateTime

        match ShardInstance.getState currentTime shardInstance with
        | ShardInstanceState.ShuttingDown shutdownAt ->
            do! ctx.CreateTimer(shutdownAt, ct)
            ctx.ContinueAsNew shardInstance

        | ShardInstanceState.Shutdown _ -> ()

        | state ->
            let events = {
                Start = ctx.WaitForExternalEvent<DateTime>(ShardInstanceEvent.Events.START, ct)
                Shutdown = ctx.WaitForExternalEvent<DateTime>(ShardInstanceEvent.Events.SHUTDOWN, ct)
            }

            match! ShardInstanceEvent.awaitAny events with
            | ShardInstanceEvent.START startAt ->
                match state with
                | ShardInstanceState.NotStarted ->
                    // TODO: Send message to node to start given instance
                    ctx.ContinueAsNew (shardInstance |> ShardInstance.start startAt)

                | _ -> ctx.ContinueAsNew shardInstance

            | ShardInstanceEvent.SHUTDOWN shutdownAt ->
                let newShardInstance = shardInstance |> ShardInstance.shutdown shutdownAt

                match state with
                | ShardInstanceState.NotStarted ->
                    ctx.ContinueAsNew newShardInstance

                | ShardInstanceState.Starting _
                | ShardInstanceState.Active ->
                    // TODO: Send message to node to shutdown given instance
                    ctx.ContinueAsNew newShardInstance

                | _ -> ctx.ContinueAsNew shardInstance

            | ShardInstanceEvent.UNKNOWN_EVENT -> ()
    }
