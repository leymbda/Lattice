namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.DurableTask.Entities
open System

type ShardInstanceEvent =
    | CREATE of startAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime
    
module ShardInstanceEvent =
    let [<Literal>] ENTITY_ID_SEPARATOR = "|"

    let [<Literal>] entityName = "ShardInstanceEntity"
    let [<Literal>] orchestratorCreateName = "ShardInstanceCreateOrchestrator"
    let [<Literal>] orchestratorShutdownName = "ShardInstanceShutdownOrchestrator"

    let parseEntityId (id: EntityInstanceId) =
        match id.Name, id.Key.Split ENTITY_ID_SEPARATOR with
        | name, [| shardId; nodeId |] when name = entityName ->
            match Guid.TryParse nodeId, ShardId.fromString shardId with
            | (true, nodeId), Some shardId -> Some (shardId, nodeId)
            | _ -> None

        | _ -> None

    let entityId (shardId: ShardId) (nodeId: Guid) =
        EntityInstanceId(entityName, nodeId.ToString() + ENTITY_ID_SEPARATOR + ShardId.toString shardId)
        
type ShardInstanceCreateInput = {
    ShardInstance: ShardInstance
    StartAt: DateTime
}

type ShardInstanceShutdownInput = {
    ShardInstance: ShardInstance
    ShutdownAt: DateTime
}
