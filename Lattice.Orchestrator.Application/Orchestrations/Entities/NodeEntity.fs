namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open Microsoft.DurableTask.Client
open System
open System.Threading.Tasks

module NodeEntity =
    let send id event (client: DurableTaskClient) =
        match event with
        | NodeEvent.CONNECT ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.CONNECT)

        | NodeEvent.DISCONNECT ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.DISCONNECT)

        | NodeEvent.HEARTBEAT shardIds ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.HEARTBEAT, shardIds)

        | NodeEvent.TRANSFER transferAt ->
            client.Entities.SignalEntityAsync(NodeEvent.entityId id, nameof NodeEvent.TRANSFER, transferAt)

    let getState id (client: DurableTaskClient) =
        client.Entities.GetEntityAsync<Node>(NodeEvent.entityId id, true)
        |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type NodeEntity (env: IEnv) =
    [<Function(NodeEvent.orchestratorDisconnectName)>]
    member _.Disconnect (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: NodeDisconnectInput
    ) =
        // Notify any remaining shards to immediately transfer if present
        input.Node.Shards
        |> List.map (fun id -> ctx.Entities.CallEntityAsync(
            ShardEvent.entityId id,
            nameof ShardEvent.TRANSFER,
            ctx.CurrentUtcDateTime))
        |> Task.WhenAll

    member _.Heartbeat shardIds node =
        node
        |> Node.heartbeat DateTime.UtcNow
        |> Node.setShards shardIds

        // TODO: Validate shards and handle disputes?

    [<Function(NodeEvent.orchestratorTransferName)>]
    member _.Transfer (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: NodeTransferInput
    ) =
        // Notify all shards to organise a transfer
        input.Node.Shards
        |> List.map (fun id -> ctx.Entities.CallEntityAsync(
            ShardEvent.entityId id,
            nameof ShardEvent.TRANSFER,
            input.TransferAt))
        |> Task.WhenAll

    // TODO: Figure out way to detect zombied nodes - where the websocket doesnt disconnect but heartbeat stops

    [<Function(NodeEvent.entityName)>]
    member this.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let id = Guid.Parse op.Context.Id.Key
                let state = op.State.GetState<Node>(Node.create id DateTime.UtcNow)

                match op.Name with
                | nameof NodeEvent.CONNECT ->
                    return state

                | nameof NodeEvent.DISCONNECT ->
                    let input: NodeDisconnectInput = {
                        Node = state
                    }

                    op.Context.ScheduleNewOrchestration(NodeEvent.orchestratorDisconnectName, input) |> ignore
                    return state

                | nameof NodeEvent.HEARTBEAT ->
                    let shardIds = op.GetInput<ShardId list>()
                    return this.Heartbeat shardIds state

                | nameof NodeEvent.TRANSFER ->
                    let input: NodeTransferInput = {
                        Node = state
                        TransferAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(NodeEvent.orchestratorTransferName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map op.State.SetState // TODO: Figure out how to handle race conditions
            |> ValueTask<obj>
        )
