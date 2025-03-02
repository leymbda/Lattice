namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open System

type ShardOrchestrator () =
    [<Function(ShardEvent.orchestratorName)>]
    static member Run (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        shard: Shard
    ) = task {
        let ct = fctx.CancellationToken

        let events = {
            CreateOrTransfer = ctx.WaitForExternalEvent<DateTime>(ShardEvent.Events.CREATE_OR_TRANSFER, ct)
            Shutdown = ctx.WaitForExternalEvent<DateTime>(ShardEvent.Events.SHUTDOWN, ct)
        }

        match! ShardEvent.awaitAny events with
        | ShardEvent.UNKNOWN_EVENT ->
            ctx.ContinueAsNew shard

        | ShardEvent.CREATE_OR_TRANSFER transferAt ->
            // TODO: ShardInstanceEvent.START transferAt
            // TODO: ShardInstanceEvent.SHUTDOWN transferAt
            ()

        | ShardEvent.SHUTDOWN shutdownAt ->
            // TODO: ShardInstanceEvent.SHUTDOWN shutdownAt
            ()
    }
