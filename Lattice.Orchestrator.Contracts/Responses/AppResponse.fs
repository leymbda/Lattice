namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type AppResponse = {
    Id: string
    PrivilegedIntents: PrivilegedIntentsResponse
    DisabledReasons: int
    Intents: int
    ShardCount: int
    Handler: HandlerResponse option
}

module AppResponse =
    let decoder: Decoder<AppResponse> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            PrivilegedIntents = get.Required.Field "privilegedIntents" PrivilegedIntentsResponse.decoder
            DisabledReasons = get.Required.Field "disabledReasons" Decode.int
            Intents = get.Required.Field "intents" Decode.int
            ShardCount = get.Required.Field "shardCount" Decode.int
            Handler = get.Optional.Field "handler" HandlerResponse.decoder
        })

    let encoder (v: AppResponse) =
        Encode.object [
            "id", Encode.string v.Id
            "privilegedIntents", PrivilegedIntentsResponse.encoder v.PrivilegedIntents
            "disabledReasons", Encode.int v.DisabledReasons
            "intents", Encode.int v.Intents
            "shardCount", Encode.int v.ShardCount
            "handler", Encode.option HandlerResponse.encoder v.Handler
        ]

    let fromDomain (v: App) = {
        Id = v.Id
        PrivilegedIntents = PrivilegedIntentsResponse.fromDomain v.PrivilegedIntents
        DisabledReasons = DisabledApplicationReason.toBitfield v.DisabledReasons
        Intents = v.Intents
        ShardCount = v.ShardCount
        Handler = v.Handler |> Option.map HandlerResponse.fromDomain
    }
