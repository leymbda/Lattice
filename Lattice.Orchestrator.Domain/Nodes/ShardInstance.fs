namespace Lattice.Orchestrator.Domain

open System

type ShardInstanceState =
    | Starting of startAt: DateTime
    | Active
    | ShuttingDown of shutdownAt: DateTime

type ShardInstance = {
    ShardId: ShardId
    NodeId: Guid
    StartAt: DateTime
    ShutdownAt: DateTime option
}

module ShardInstance =
    let create shardId nodeId startAt =
        {
            ShardId = shardId
            NodeId = nodeId
            StartAt = startAt
            ShutdownAt = None
        }

    let shutdown shutdownAt shardInstance =
        { shardInstance with ShutdownAt = Some shutdownAt }

    let getState currentTime shardInstance =
        match shardInstance with
        | { StartAt = startAt } when startAt > currentTime -> Starting startAt
        | { ShutdownAt = Some shutdownAt } when shutdownAt > currentTime -> ShuttingDown shutdownAt
        | _ -> Active
