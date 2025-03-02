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
        let currentTime = ctx.CurrentUtcDateTime

        match Node.getState currentTime node with
        | NodeState.Expired ->
            // TODO: Handle expired node (immediately transfer all shards)
            ()

        | NodeState.Active ->
            let events = {
                StartInstance = ctx.WaitForExternalEvent<ShardId * DateTime>(NodeEvent.Events.START_INSTANCE, ct)
                ShutdownInstance = ctx.WaitForExternalEvent<ShardId * DateTime>(NodeEvent.Events.SHUTDOWN_INSTANCE, ct)
                SendInstanceEvent = ctx.WaitForExternalEvent<ShardId>(NodeEvent.Events.SEND_INSTANCE_EVENT, ct)
                TransferAllInstances = ctx.WaitForExternalEvent(NodeEvent.Events.TRANSFER_ALL_INSTANCES, ct)
                HeartbeatAck = ctx.WaitForExternalEvent<DateTime>(NodeEvent.Events.HEARTBEAT_ACK, ct)
                HeartbeatTimeout = ctx.CreateTimer(node.LastHeartbeatAck.AddSeconds Node.LIFETIME_SECONDS, ct)
            }

            // TODO: Should timers be created to handle timeouts for instructions with time limits? Instance start and
            //       shutdown currently instantly remove shard IDs which could cause issues.

            match! NodeEvent.awaitAny events with
            | NodeEvent.START_INSTANCE (shardId, startAt) ->
                // TODO: Notify node to start a shard instance at the given time
                ctx.ContinueAsNew (node |> Node.addStard shardId)

            | NodeEvent.SHUTDOWN_INSTANCE (shardId, shutdownAt) ->
                // TODO: Notify node to shutdown a shard instance at the given time
                ctx.ContinueAsNew (node |> Node.removeShard shardId)

            | NodeEvent.SEND_INSTANCE_EVENT shardId ->
                // TODO: Send gateway event through node
                ctx.ContinueAsNew node

            | NodeEvent.TRANSFER_ALL_INSTANCES ->
                // TODO: Gracefully transfer all shards (ShardEvent.CREATE_OR_TRANSFER shardId)
                ctx.ContinueAsNew node

            | NodeEvent.HEARTBEAT_ACK sentAt ->
                ctx.ContinueAsNew (node |> Node.heartbeat sentAt)
                ctx.ContinueAsNew node

            | NodeEvent.HEARTBEAT_TIMEOUT ->
                ctx.ContinueAsNew node

            | NodeEvent.UNKNOWN_EVENT -> ()
    }
