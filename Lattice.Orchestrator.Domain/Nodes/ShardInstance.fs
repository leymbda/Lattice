namespace Lattice.Orchestrator.Domain

open System

type ShardInstanceState =
    | NotStarted
    | Starting of startAt: DateTime
    | Active
    | ShuttingDown of shutdownAt: DateTime
    | Shutdown of shutdownAt: DateTime

type ShardInstance = {
    ShardId: ShardId
    NodeId: Guid
    StartAt: DateTime option
    ShutdownAt: DateTime option
}

module ShardInstance =
    let create shardId nodeId =
        {
            ShardId = shardId
            NodeId = nodeId
            StartAt = None
            ShutdownAt = None
        }

    let start startAt shardInstance =
        { shardInstance with StartAt = Some startAt }

    let shutdown shutdownAt shardInstance =
        { shardInstance with ShutdownAt = Some shutdownAt }

    let getState currentTime shardInstance =
        match shardInstance with
        | { StartAt = None } -> NotStarted
        | { StartAt = Some startAt; ShutdownAt = None } when startAt > currentTime -> Starting startAt
        | { ShutdownAt = None } -> Active
        | { ShutdownAt = Some shutdownAt } when shutdownAt > currentTime -> ShuttingDown shutdownAt
        | { ShutdownAt = Some shutdownAt } -> Shutdown shutdownAt
