namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type ShardInstanceSendScheduleStartMessage = {
    NodeId: Guid
    ShardId: ShardId
    Token: string
    Intents: int
    StartAt: DateTime
}

module ShardInstanceSendScheduleStartMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShardId = "shardId"
        let [<Literal>] Token = "token"
        let [<Literal>] Intents = "intents"
        let [<Literal>] StartAt = "startAt"

    let decoder: Decoder<ShardInstanceSendScheduleStartMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShardId = get.Required.Field Property.ShardId ShardId.decoder
            Token = get.Required.Field Property.Token Decode.string
            Intents = get.Required.Field Property.Intents Decode.int
            StartAt = get.Required.Field Property.StartAt Decode.datetimeUtc
        })

    let encoder (v: ShardInstanceSendScheduleStartMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShardId, ShardId.encoder v.ShardId
            Property.Token, Encode.string v.Token
            Property.Intents, Encode.int v.Intents
            Property.StartAt, Encode.datetime v.StartAt
        ]
    