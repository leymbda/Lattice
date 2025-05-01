namespace Lattice.Orchestrator.Contracts

open System
open Thoth.Json.Net

type NodeReceiveHeartbeatMessage = {
    NodeId: Guid
    Shards: string list
}

module NodeReceiveHeartbeatMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] Shards = "shards"

    let decoder: Decoder<NodeReceiveHeartbeatMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            Shards = get.Required.Field Property.Shards (Decode.list Decode.string)
        })

    let encoder (v: NodeReceiveHeartbeatMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.Shards, (List.map Encode.string >> Encode.list) v.Shards
        ]
    