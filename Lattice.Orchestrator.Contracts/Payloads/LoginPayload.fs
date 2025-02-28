namespace Lattice.Orchestrator.Contracts

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

    let encoder (v: LoginPayload) =
        Encode.object [
            "code", Encode.string v.Code
            "redirectUri", Encode.string v.RedirectUri
        ]
