namespace Lattice.Orchestrator.Contracts

open System
open Thoth.Json.Net

type NodeConnectedMessage = {
    NodeId: Guid
    ConnectedAt: DateTime
}

module NodeConnectedMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ConnectedAt = "connectedAt"

    let decoder: Decoder<NodeConnectedMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ConnectedAt = get.Required.Field Property.ConnectedAt Decode.datetimeUtc
        })

    let encoder (v: NodeConnectedMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ConnectedAt, Encode.datetime v.ConnectedAt
        ]
    