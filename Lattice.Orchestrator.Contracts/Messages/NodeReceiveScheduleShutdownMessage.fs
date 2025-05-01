namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type NodeReceiveScheduleShutdownMessage = {
    NodeId: Guid
    ShutdownAt: DateTime
    ShardIds: ShardId list
}

module NodeReceiveScheduleShutdownMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShardIds = "shardIds"
        let [<Literal>] ShutdownAt = "shutdownAt"

    let decoder: Decoder<NodeReceiveScheduleShutdownMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShardIds = get.Required.Field Property.ShardIds (Decode.list ShardId.decoder)
            ShutdownAt = get.Required.Field Property.ShutdownAt Decode.datetimeUtc
        })

    let encoder (v: NodeReceiveScheduleShutdownMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShardIds, (List.map ShardId.encoder >> Encode.list) v.ShardIds
            Property.ShutdownAt, Encode.datetime v.ShutdownAt
        ]
    