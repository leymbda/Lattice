﻿namespace Lattice.Orchestrator.Domain

open System

type NodeState =
    | Active
    | Expired

type Node = {
    Id: Guid
    LastHeartbeatAck: DateTime
    Shards: ShardId list
}

module Node =
    let [<Literal>] NODE_HEARTBEAT_FREQUENCY_SECS = 30
    let [<Literal>] LIFETIME_SECONDS = NODE_HEARTBEAT_FREQUENCY_SECS * 2

    let create id currentTime = {
        Id = id
        LastHeartbeatAck = currentTime
        Shards = []
    }

    let heartbeat currentTime node =
        { node with LastHeartbeatAck = currentTime }

    let addShard shardId node =
        { node with Shards = node.Shards @ [shardId] |> List.distinct }

    let removeShard shardId node =
        { node with Shards = node.Shards |> List.filter ((<>) shardId) }

    // TODO: How to handle graceful (delayed) addition and removal of shards in the context of the domain?
        
    let getState (currentTime: DateTime) node =
        match node with
        | { LastHeartbeatAck = lastHeartbeatAck } when lastHeartbeatAck.AddSeconds LIFETIME_SECONDS < currentTime -> Expired
        | _ -> Active
