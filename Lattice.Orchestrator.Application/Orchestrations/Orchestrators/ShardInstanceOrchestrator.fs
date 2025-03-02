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

        let events = {
            Start = ctx.WaitForExternalEvent<DateTime>(ShardInstanceEvent.Events.START, ct)
            Shutdown = ctx.WaitForExternalEvent<DateTime>(ShardInstanceEvent.Events.SHUTDOWN, ct)
            SendEvent = ctx.WaitForExternalEvent(ShardInstanceEvent.Events.SEND_EVENT, ct)
        }

        match! ShardInstanceEvent.awaitAny events with
        | ShardInstanceEvent.UNKNOWN_EVENT ->
            ctx.ContinueAsNew shardInstance

        | ShardInstanceEvent.START startAt ->
            // TODO: NodeEvent.START_INSTANCE startAt
            ()

        | ShardInstanceEvent.SHUTDOWN shutdownAt ->
            // TODO: NodeEvent.SHUTDOWN_INSTANCE shutdownAt
            ()

        | ShardInstanceEvent.SEND_EVENT ->
            // TODO: NodeEvent.SEND_INSTANCE_EVENT
            ()
    }
