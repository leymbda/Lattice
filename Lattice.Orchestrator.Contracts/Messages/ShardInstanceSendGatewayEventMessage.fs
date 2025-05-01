namespace Lattice.Orchestrator.Contracts

open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type ShardInstanceSendGatewayEventMessage = {
    NodeId: Guid
    ShardId: ShardId
    Event: GatewaySendEvent
}

module ShardInstanceSendGatewayEventMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShardId = "shardId"
        let [<Literal>] Event = "event"

    let decoder: Decoder<ShardInstanceSendGatewayEventMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShardId = get.Required.Field Property.ShardId ShardId.decoder
            Event = get.Required.Field Property.Event GatewaySendEvent.decoder
        })

    let encoder (v: ShardInstanceSendGatewayEventMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShardId, ShardId.encoder v.ShardId
            //Property.Event, GatewaySendEvent.encoder v.Event // TODO: Implement in FSharp.Discord
        ]
    