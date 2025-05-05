namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type NodeReceiveHeartbeatMessage = {
    NodeId: Guid
    ShardIds: ShardId list
}

module NodeReceiveHeartbeatMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShardIds = "shardIds"

    let decoder: Decoder<NodeReceiveHeartbeatMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShardIds = get.Required.Field Property.ShardIds (Decode.list ShardId.decoder)
        })

    let encoder (v: NodeReceiveHeartbeatMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShardIds, (List.map ShardId.encoder >> Encode.list) v.ShardIds
        ]
    