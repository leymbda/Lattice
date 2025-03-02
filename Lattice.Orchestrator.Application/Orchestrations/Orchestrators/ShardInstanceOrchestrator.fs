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
                SendEvent = ctx.WaitForExternalEvent(ShardInstanceEvent.Events.SEND_EVENT, ct)
            }

            match! ShardInstanceEvent.awaitAny events with
            | ShardInstanceEvent.START startAt ->
                match state with
                | ShardInstanceState.NotStarted ->
                    ctx.SendEvent(NodeEvent.orchestratorId shardInstance.NodeId, NodeEvent.Events.START_INSTANCE, (shardInstance.ShardId, startAt))
                    ctx.ContinueAsNew (shardInstance |> ShardInstance.start startAt)

                | _ -> ctx.ContinueAsNew shardInstance

            | ShardInstanceEvent.SHUTDOWN shutdownAt ->
                let newShardInstance = shardInstance |> ShardInstance.shutdown shutdownAt

                match state with
                | ShardInstanceState.NotStarted ->
                    ctx.ContinueAsNew newShardInstance

                | ShardInstanceState.Starting _
                | ShardInstanceState.Active ->
                    ctx.SendEvent(NodeEvent.orchestratorId shardInstance.NodeId, NodeEvent.Events.SHUTDOWN_INSTANCE, (shardInstance.ShardId, shutdownAt))
                    ctx.ContinueAsNew newShardInstance

                | _ -> ctx.ContinueAsNew shardInstance

            | ShardInstanceEvent.SEND_EVENT ->
                match state with
                | ShardInstanceState.Active ->
                    ctx.SendEvent(NodeEvent.orchestratorId shardInstance.NodeId, NodeEvent.Events.SEND_INSTANCE_EVENT, (shardInstance.ShardId))
                    ctx.ContinueAsNew shardInstance
                
                | _ -> ctx.ContinueAsNew shardInstance

            | ShardInstanceEvent.UNKNOWN_EVENT -> ()
    }
