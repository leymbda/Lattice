namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

type ShardInstanceResponse = {
    Id: ShardId
    AppId: string
    NodeId: Guid
    Status: ShardInstanceStatus
}

module ShardInstanceResponse =
    module Property =
        let [<Literal>] Id = "id"
        let [<Literal>] AppId = "appId"
        let [<Literal>] NodeId = "nodeId"
        let [<Literal>] Status = "status"

    let decoder: Decoder<ShardInstanceResponse> =
        Decode.object (fun get -> {
            Id = get.Required.Field Property.Id ShardId.decoder
            AppId = get.Required.Field Property.AppId Decode.string
            NodeId = get.Required.Field Property.NodeId Decode.guid
            Status = get.Required.Field Property.Status Decode.Enum.int<ShardInstanceStatus>
        })

    let encoder (v: ShardInstanceResponse) =
        Encode.object [
            Property.Id, ShardId.encoder v.Id
            Property.AppId, Encode.string v.AppId
            Property.NodeId, Encode.guid v.NodeId
            Property.Status, Encode.Enum.int v.Status
        ]

    let fromDomain currentTime (v: ShardInstance) = {
        Id = v.ShardId
        AppId = v.ShardId |> function ShardId (appId, _, _) -> appId
        NodeId = v.NodeId
        Status = ShardInstance.getState currentTime v |> ShardInstanceStatus.fromDomain
    }
