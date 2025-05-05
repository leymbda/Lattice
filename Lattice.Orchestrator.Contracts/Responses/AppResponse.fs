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
    module Property =
        let [<Literal>] Id = "id"
        let [<Literal>] PrivilegedIntents = "privilegedIntents"
        let [<Literal>] DisabledReasons = "disabledReasons"
        let [<Literal>] Intents = "intents"
        let [<Literal>] ShardCount = "shardCount"
        let [<Literal>] Handler = "handler"

    let decoder: Decoder<AppResponse> =
        Decode.object (fun get -> {
            Id = get.Required.Field Property.Id Decode.string
            PrivilegedIntents = get.Required.Field Property.PrivilegedIntents PrivilegedIntentsResponse.decoder
            DisabledReasons = get.Required.Field Property.DisabledReasons Decode.int
            Intents = get.Required.Field Property.Intents Decode.int
            ShardCount = get.Required.Field Property.ShardCount Decode.int
            Handler = get.Optional.Field Property.Handler HandlerResponse.decoder
        })

    let encoder (v: AppResponse) =
        Encode.object [
            Property.Id, Encode.string v.Id
            Property.PrivilegedIntents, PrivilegedIntentsResponse.encoder v.PrivilegedIntents
            Property.DisabledReasons, Encode.int v.DisabledReasons
            Property.Intents, Encode.int v.Intents
            Property.ShardCount, Encode.int v.ShardCount
            Property.Handler, Encode.option HandlerResponse.encoder v.Handler
        ]

    let fromDomain (v: App) = {
        Id = v.Id
        PrivilegedIntents = PrivilegedIntentsResponse.fromDomain v.PrivilegedIntents
        DisabledReasons = DisabledAppReason.toBitfield v.DisabledReasons
        Intents = v.Intents
        ShardCount = v.ShardCount
        Handler = v.Handler |> Option.map HandlerResponse.fromDomain
    }
