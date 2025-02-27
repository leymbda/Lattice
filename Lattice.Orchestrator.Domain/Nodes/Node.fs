namespace Lattice.Orchestrator.Domain

open System

type Node = {
    Id: Guid
    Shards: Guid list
}

module Node =
    let create id = {
        Id = id
        Shards = []
    }

    let addShard shardId node =
        { node with Shards = shardId :: node.Shards }

    let removeShard shardId node =
        { node with Shards = List.filter (fun id -> id <> shardId) node.Shards }
