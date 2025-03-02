namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open System

type NodeOrchestrator () =
    [<Function(NodeEvent.orchestratorName)>]
    static member Run (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        node: Node
    ) = task {
        let ct = fctx.CancellationToken

        let lastHeartbeatAck = ctx.CurrentUtcDateTime // TODO: Get from node state

        let events = {
            StartInstance = ctx.WaitForExternalEvent<ShardId * DateTime>(NodeEvent.Events.START_INSTANCE, ct)
            ShutdownInstance = ctx.WaitForExternalEvent<ShardId * DateTime>(NodeEvent.Events.SHUTDOWN_INSTANCE, ct)
            TransferInstance = ctx.WaitForExternalEvent<ShardId * DateTime>(NodeEvent.Events.TRANSFER_INSTANCE, ct)
            SendInstanceEvent = ctx.WaitForExternalEvent<ShardId>(NodeEvent.Events.SEND_INSTANCE_EVENT, ct)
            HeartbeatAck = ctx.WaitForExternalEvent<DateTime>(NodeEvent.Events.HEARTBEAT_ACK, ct)
            HeartbeatTimeout = ctx.CreateTimer(lastHeartbeatAck.AddSeconds Node.LIFETIME_SECONDS, ct)
        }

        match! NodeEvent.awaitAny events with
        | NodeEvent.UNKNOWN_EVENT ->
            ctx.ContinueAsNew node

        | NodeEvent.START_INSTANCE (shardId, startAt) ->
            // TODO: Notify node to start a shard instance
            ()

        | NodeEvent.SHUTDOWN_INSTANCE (shardId, shutdownAt) ->
            // TODO: Notify node to shutdown a shard instance
            ()

        | NodeEvent.TRANSFER_INSTANCE (shardId, transferAt) ->
            // TODO: ShardEvent.CREATE_OR_TRANSFER transferAt
            ()

        | NodeEvent.SEND_INSTANCE_EVENT shardId ->
            // TODO: Send gateway event through node
            ()

        | NodeEvent.HEARTBEAT_ACK sentAt ->
            // TODO: Handle heartbeat
            ()

        | NodeEvent.HEARTBEAT_TIMEOUT ->
            // TODO: Trigger instant transfer to recover dead shard instance
            ()
    }
