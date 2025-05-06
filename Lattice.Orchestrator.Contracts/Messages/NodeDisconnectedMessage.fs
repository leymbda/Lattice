namespace Lattice.Orchestrator.Contracts

open System
open Thoth.Json.Net

type NodeDisconnectedMessage = {
    NodeId: Guid
    DisconnectedAt: DateTime
}

module NodeDisconnectedMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] DisconnectedAt = "disconnectedAt"

    let decoder: Decoder<NodeDisconnectedMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            DisconnectedAt = get.Required.Field Property.DisconnectedAt Decode.datetimeUtc
        })

    let encoder (v: NodeDisconnectedMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.DisconnectedAt, Encode.datetime v.DisconnectedAt
        ]
    