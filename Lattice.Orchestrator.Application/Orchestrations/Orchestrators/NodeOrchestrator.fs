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
                TransferAllInstances = ctx.WaitForExternalEvent(NodeEvent.Events.TRANSFER_ALL_INSTANCES, ct)
                HeartbeatAck = ctx.WaitForExternalEvent<DateTime>(NodeEvent.Events.HEARTBEAT_ACK, ct)
                HeartbeatTimeout = ctx.CreateTimer(node.LastHeartbeatAck.AddSeconds Node.LIFETIME_SECONDS, ct)
            }

            // TODO: Should timers be created to handle timeouts for instructions with time limits? Instance start and
            //       shutdown currently instantly remove shard IDs which could cause issues.

            match! NodeEvent.awaitAny events with
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
