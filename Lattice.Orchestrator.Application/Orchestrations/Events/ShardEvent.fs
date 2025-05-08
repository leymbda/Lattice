namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.DurableTask.Entities
open System

type ShardEvent =
    | CREATE of startAt: DateTime
    | TRANSFER of transferAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime

module ShardEvent =
    let [<Literal>] entityName = "ShardEntity"
    let [<Literal>] orchestratorCreateName = "ShardCreateOrchestrator"
    let [<Literal>] orchestratorTransferName = "ShardTransferOrchestrator"
    let [<Literal>] orchestratorShutdownName = "ShardShutdownOrchestrator"

    let entityId (shardId: ShardId) =
        EntityInstanceId(entityName, ShardId.toString shardId)

type ShardCreateInput = {
    Shard: Shard
    StartAt: DateTime
}

type ShardTransferInput = {
    Shard: Shard
    TransferAt: DateTime
}

type ShardShutdownInput = {
    Shard: Shard
    ShutdownAt: DateTime
}
