namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.DurableTask.Entities
open System

type ShardEvent =
    | CREATE_OR_TRANSFER of transferAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime

module ShardEvent =
    let [<Literal>] entityName = "ShardEntity"

    let entityId (shardId: ShardId) =
        EntityInstanceId(entityName, ShardId.toString shardId)
