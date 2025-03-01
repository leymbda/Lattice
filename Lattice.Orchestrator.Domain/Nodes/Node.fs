namespace Lattice.Orchestrator.Domain

open System

type Node = {
    Id: Guid
    Shards: ShardId list
    LastHeartbeatAck: DateTime
}

module Node =
    let [<Literal>] NODE_HEARTBEAT_FREQUENCY_SECS = 30
    let [<Literal>] LIFETIME_SECONDS = NODE_HEARTBEAT_FREQUENCY_SECS * 2

    let create id currentTime = {
        Id = id
        Shards = []
        LastHeartbeatAck = currentTime
    }

    let addShard shardId node =
        { node with Shards = node.Shards @ [shardId] |> List.distinct }

    let removeShard shardId node =
        { node with Shards = node.Shards |> List.filter ((<>) shardId) }

    let heartbeat currentTime node =
        { node with LastHeartbeatAck = currentTime }
        
    let isAlive currentTime node =
        (currentTime - node.LastHeartbeatAck).TotalSeconds < float LIFETIME_SECONDS

    let isTransferReady node =
        List.isEmpty node.Shards
