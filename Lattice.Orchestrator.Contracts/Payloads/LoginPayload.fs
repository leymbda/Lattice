namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type LoginPayload = {
    Code: string
    RedirectUri: string
}

module LoginPayload =
    module Property =
        let [<Literal>] Code = "code"
        let [<Literal>] RedirectUri = "redirectUri"

    let decoder: Decoder<LoginPayload> =
        Decode.object (fun get -> {
            Code = get.Required.Field Property.Code Decode.string
            RedirectUri = get.Required.Field Property.RedirectUri Decode.string
        })

    let encoder (v: LoginPayload) =
        Encode.object [
            Property.Code, Encode.string v.Code
            Property.RedirectUri, Encode.string v.RedirectUri
        ]
