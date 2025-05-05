namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type ShardResponse = {
    Id: ShardId
    AppId: string
    Status: ShardStatus
}

module ShardResponse =
    module Property =
        let [<Literal>] Id = "id"
        let [<Literal>] AppId = "appId"
        let [<Literal>] Status = "status"

    let decoder: Decoder<ShardResponse> =
        Decode.object (fun get -> {
            Id = get.Required.Field Property.Id ShardId.decoder
            AppId = get.Required.Field Property.AppId Decode.string
            Status = get.Required.Field Property.Status Decode.Enum.int<ShardStatus>
        })

    let encoder (v: ShardResponse) =
        Encode.object [
            Property.Id, ShardId.encoder v.Id
            Property.AppId, Encode.string v.AppId
            Property.Status, Encode.Enum.int v.Status
        ]

    let fromDomain currentTime (v: Shard) = {
        Id = v.Id
        AppId = v.Id |> function ShardId (appId, _, _) -> appId
        Status = Shard.getState currentTime v |> ShardStatus.fromDomain
    }
