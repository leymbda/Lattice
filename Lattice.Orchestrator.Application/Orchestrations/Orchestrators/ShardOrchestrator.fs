﻿namespace Lattice.Orchestrator.Application

open FSharp.Discord.Gateway
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
        let currentTime = ctx.CurrentUtcDateTime

        match Shard.getState currentTime shard with
        | ShardState.ShuttingDown (_, shutdownAt) ->
            do! ctx.CreateTimer(shutdownAt, ct)
            ctx.ContinueAsNew shard

        | ShardState.Shutdown _ -> ()

        | state ->
            let events = {
                CreateOrTransfer = ctx.WaitForExternalEvent<DateTime>(ShardEvent.Events.CREATE_OR_TRANSFER, ct)
                SendGatewayEvent = ctx.WaitForExternalEvent<GatewaySendEvent>(ShardEvent.Events.SEND_GATEWAY_EVENT, ct)
                Shutdown = ctx.WaitForExternalEvent<DateTime>(ShardEvent.Events.SHUTDOWN, ct)
            }

            match! ShardEvent.awaitAny events with
            | ShardEvent.CREATE_OR_TRANSFER transferAt ->
                let upsert current startAt (shard: Shard) (ctx: TaskOrchestrationContext) = task {
                    let next = Guid.NewGuid() // TODO: Start new shard instance orchestrator (suborchestrator)

                    ctx.SendEvent(ShardInstanceEvent.orchestratorId shard.Id next, ShardInstanceEvent.Events.START, startAt)

                    match current with
                    | None -> ()
                    | Some current -> ctx.SendEvent(ShardInstanceEvent.orchestratorId shard.Id current, ShardInstanceEvent.Events.SHUTDOWN, transferAt)

                    return shard |> Shard.addInstance next transferAt
                }

                match state with
                | ShardState.NotStarted ->
                    let! newShard = ctx |> upsert None transferAt shard
                    ctx.ContinueAsNew newShard

                | ShardState.Active current ->
                    let! newShard = ctx |> upsert (Some current) transferAt shard
                    ctx.ContinueAsNew newShard

                | _ -> ctx.ContinueAsNew shard

            | ShardEvent.SEND_GATEWAY_EVENT event ->
                match state with
                | ShardState.Active current ->
                    // TODO: Send gateway event to node with instance to forward to Discord
                    ctx.ContinueAsNew shard

                | ShardState.Transferring (current, next, transferAt) ->
                    // TODO: Send gateway event to node with instance to forward to Discord
                    // TODO: How should this handle when instance is soon to transfer?
                    ctx.ContinueAsNew shard
                
                | _ -> ctx.ContinueAsNew shard

            | ShardEvent.SHUTDOWN shutdownAt ->
                let newShard = shard |> Shard.shutdown shutdownAt

                match state with
                | ShardState.NotStarted ->
                    ctx.ContinueAsNew newShard

                | ShardState.Starting (current, _)
                | ShardState.Active current ->
                    ctx.SendEvent(ShardInstanceEvent.orchestratorId shard.Id current, ShardInstanceEvent.Events.SHUTDOWN, shutdownAt)
                    ctx.ContinueAsNew newShard

                | ShardState.Transferring (current, next, _) ->
                    ctx.SendEvent(ShardInstanceEvent.orchestratorId shard.Id current, ShardInstanceEvent.Events.SHUTDOWN, shutdownAt)
                    ctx.SendEvent(ShardInstanceEvent.orchestratorId shard.Id next, ShardInstanceEvent.Events.SHUTDOWN, shutdownAt)
                    ctx.ContinueAsNew newShard

                | _ -> ctx.ContinueAsNew shard

            | ShardEvent.UNKNOWN_EVENT -> ()
    }
