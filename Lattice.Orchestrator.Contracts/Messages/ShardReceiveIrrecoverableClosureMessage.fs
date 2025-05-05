namespace Lattice.Orchestrator.Contracts

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type ShardReceiveIrrecoverableClosureMessage = {
    NodeId: Guid
    ShardId: ShardId
    Code: GatewayCloseEventCode
}

module ShardReceiveIrrecoverableClosureMessage =
    module Property =
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] ShardId = "shardId"
        let [<Literal>] Code = "code"

    let decoder: Decoder<ShardReceiveIrrecoverableClosureMessage> =
        Decode.object (fun get -> {
            NodeId = get.Required.Field Property.NodeId Decode.guid
            ShardId = get.Required.Field Property.ShardId ShardId.decoder
            Code = get.Required.Field Property.Code Decode.Enum.int<GatewayCloseEventCode>
        })

    let encoder (v: ShardReceiveIrrecoverableClosureMessage) =
        Encode.object [
            Property.NodeId, Encode.guid v.NodeId
            Property.ShardId, ShardId.encoder v.ShardId
            Property.Code, Encode.Enum.int v.Code
        ]
    