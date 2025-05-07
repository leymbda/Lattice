namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.DurableTask.Entities
open System

type NodeEvent =
    | CONNECT
    | DISCONNECT
    | HEARTBEAT of shardIds: ShardId list
    | TRANSFER_ALL_INSTANCES of transferAt: DateTime
    
module NodeEvent =
    let [<Literal>] entityName = "NodeEntity"

    let entityId (id: Guid) =
        EntityInstanceId(entityName, id.ToString())
