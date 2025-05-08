namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
//open Microsoft.DurableTask.Client
open Microsoft.DurableTask.Entities
open System
open System.Threading.Tasks

type NodeEvent =
    | GET
    | CONNECT
    | DISCONNECT
    | HEARTBEAT of shardIds: ShardId list
    | TRANSFER of transferAt: DateTime
    
type NodeDisconnectInput = {
    NodeId: Guid
}

type NodeTransferInput = {
    NodeId: Guid
    TransferAt: DateTime
}

module NodeEntity =
    let [<Literal>] entityName = "NodeEntity"
    let [<Literal>] disconnectOrchestratorName = "NodeDisconnectOrchestrator"
    let [<Literal>] transferOrchestratorName = "NodeTransferOrchestrator"

    let entityId (id: Guid) =
        EntityInstanceId(entityName, id.ToString())

    //let send id event (client: DurableTaskClient) =
    //    match event with
    //    | NodeEvent.CONNECT ->
    //        client.Entities.SignalEntityAsync(entityId id, nameof NodeEvent.CONNECT)

    //    | NodeEvent.DISCONNECT ->
    //        client.Entities.SignalEntityAsync(entityId id, nameof NodeEvent.DISCONNECT)

    //    | NodeEvent.HEARTBEAT shardIds ->
    //        client.Entities.SignalEntityAsync(entityId id, nameof NodeEvent.HEARTBEAT, shardIds)

    //    | NodeEvent.TRANSFER transferAt ->
    //        client.Entities.SignalEntityAsync(entityId id, nameof NodeEvent.TRANSFER, transferAt)

    //let getState id (client: DurableTaskClient) =
    //    client.Entities.GetEntityAsync<Node>(entityId id, true)
    //    |> Task.map (fun e -> e.IncludesState |> function | true -> Some e.State | false -> None)

type NodeEntity (env: IEnv) =
    [<Function(NodeEntity.disconnectOrchestratorName)>]
    member _.Disconnect (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: NodeDisconnectInput
    ) = task {
        // Get current entity state
        let! node = ctx.Entities.CallEntityAsync<Node>(NodeEntity.entityId input.NodeId, nameof NodeEvent.GET)

        // Notify any remaining shards to immediately transfer if present
        return!
            node.Shards
            |> List.map (fun id -> ctx.Entities.CallEntityAsync(
                ShardEntity.entityId id,
                nameof ShardEvent.TRANSFER,
                ctx.CurrentUtcDateTime))
            |> Task.WhenAll
    }

    member _.Heartbeat shardIds node =
        node
        |> Node.heartbeat DateTime.UtcNow
        |> Node.setShards shardIds

        // TODO: Validate shards and handle disputes?

    [<Function(NodeEntity.transferOrchestratorName)>]
    member _.Transfer (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        input: NodeTransferInput
    ) = task {
        // Get current entity state
        let! node = ctx.Entities.CallEntityAsync<Node>(NodeEntity.entityId input.NodeId, nameof NodeEvent.GET)

        // Notify all shards to organise a transfer
        return!
            node.Shards
            |> List.map (fun id -> ctx.Entities.CallEntityAsync(
                ShardEntity.entityId id,
                nameof ShardEvent.TRANSFER,
                input.TransferAt))
            |> Task.WhenAll
    }

    // TODO: Figure out way to detect zombied nodes - where the websocket doesnt disconnect but heartbeat stops

    [<Function(NodeEntity.entityName)>]
    member this.DispatchAsync ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync(fun op ->
            task {
                let id = Guid.Parse op.Context.Id.Key
                let state = op.State.GetState<Node>(Node.create id DateTime.UtcNow)

                match op.Name with
                | nameof NodeEvent.GET ->
                    return state

                | nameof NodeEvent.CONNECT ->
                    return state

                | nameof NodeEvent.DISCONNECT ->
                    let input: NodeDisconnectInput = {
                        NodeId = state.Id
                    }

                    op.Context.ScheduleNewOrchestration(NodeEntity.disconnectOrchestratorName, input) |> ignore
                    return state

                | nameof NodeEvent.HEARTBEAT ->
                    let shardIds = op.GetInput<ShardId list>()
                    return this.Heartbeat shardIds state

                | nameof NodeEvent.TRANSFER ->
                    let input: NodeTransferInput = {
                        NodeId = state.Id
                        TransferAt = op.GetInput<DateTime>()
                    }

                    op.Context.ScheduleNewOrchestration(NodeEntity.transferOrchestratorName, input) |> ignore
                    return state

                | _ -> return state
            }
            |> Task.map (fun s -> op.State.SetState s; s)
            |> ValueTask<obj>
        )
        
    // TODO: Handle deleting entity by setting state to null (how to handle async through shutdown orchestrations?)
