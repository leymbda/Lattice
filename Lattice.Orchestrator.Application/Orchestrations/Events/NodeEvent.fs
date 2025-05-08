namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.DurableTask.Entities
open System

type NodeEvent =
    | CONNECT
    | DISCONNECT
    | HEARTBEAT of shardIds: ShardId list
    | TRANSFER of transferAt: DateTime
    
module NodeEvent =
    let [<Literal>] entityName = "NodeEntity"
    let [<Literal>] orchestratorDisconnectName = "NodeDisconnectOrchestrator"
    let [<Literal>] orchestratorTransferName = "NodeTransferOrchestrator"

    let entityId (id: Guid) =
        EntityInstanceId(entityName, id.ToString())

type NodeDisconnectInput = {
    Node: Node
}

type NodeTransferInput = {
    Node: Node
    TransferAt: DateTime
}
