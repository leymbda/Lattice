namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type ApplicationResponse = {
    Id: string
    PrivilegedIntents: PrivilegedIntentsResponse
    DisabledReasons: int
    Intents: int
    ProvisionedShardCount: int
    Handler: HandlerResponse
}

module ApplicationResponse =
    let encoder (v: ApplicationResponse) =
        Encode.object [
            "id", Encode.string v.Id
            "privilegedIntents", PrivilegedIntentsResponse.encoder v.PrivilegedIntents
            "disabledReasons", Encode.int v.DisabledReasons
            "intents", Encode.int v.Intents
            "provisionedShardCount", Encode.int v.ProvisionedShardCount
            "handler", HandlerResponse.encoder v.Handler
        ]

    let fromDomain (v: Application) = {
        Id = v.Id
        PrivilegedIntents = PrivilegedIntentsResponse.fromDomain v.PrivilegedIntents
        DisabledReasons = DisabledApplicationReason.toBitfield v.DisabledReasons
        Intents = v.Intents
        ProvisionedShardCount = v.ProvisionedShardCount
        Handler = HandlerResponse.fromDomain v.Handler
    }
