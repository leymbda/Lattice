namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type ShardInstanceSendScheduleCloseMessage = {
    NodeId: Guid
    ShardId: ShardId
    CloseAt: DateTime
}

module ShardInstanceSendScheduleCloseMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShardId = "shardId"
        let [<Literal>] CloseAt = "closeAt"

    let decoder: Decoder<ShardInstanceSendScheduleCloseMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShardId = get.Required.Field Property.ShardId ShardId.decoder
            CloseAt = get.Required.Field Property.CloseAt Decode.datetimeUtc
        })

    let encoder (v: ShardInstanceSendScheduleCloseMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShardId, ShardId.encoder v.ShardId
            Property.CloseAt, Encode.datetime v.CloseAt
        ]
    