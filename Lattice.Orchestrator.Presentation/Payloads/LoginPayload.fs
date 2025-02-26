namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type LoginPayload = {
    Code: string
    RedirectUri: string
}

module LoginPayload =
    let decoder: Decoder<LoginPayload> =
        Decode.object (fun get -> {
            Code = get.Required.Field "code" Decode.string
            RedirectUri = get.Required.Field "redirectUri" Decode.string
        })
