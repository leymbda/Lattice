namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type TokenResponse = {
    Token: string
}

module TokenResponse =
    let encoder (v: TokenResponse) =
        Encode.object [
            "token", Encode.string v.Token
        ]

    let fromDomain (token: string) = {
        Token = token
    }
