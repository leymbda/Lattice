namespace Lattice.Orchestrator.Contracts

open System
open Thoth.Json.Net

type NodeReceiveScheduleShutdownMessage = {
    NodeId: Guid
    ShutdownAt: DateTime
    Shards: string list
}

module NodeReceiveScheduleShutdownMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShutdownAt = "shutdownAt"
        let [<Literal>] Shards = "shards"

    let decoder: Decoder<NodeReceiveScheduleShutdownMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShutdownAt = get.Required.Field Property.ShutdownAt Decode.datetimeUtc
            Shards = get.Required.Field Property.Shards (Decode.list Decode.string)
        })

    let encoder (v: NodeReceiveScheduleShutdownMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShutdownAt, Encode.datetime v.ShutdownAt
            Property.Shards, (List.map Encode.string >> Encode.list) v.Shards
        ]
    