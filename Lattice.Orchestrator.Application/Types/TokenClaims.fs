namespace Lattice.Orchestrator.Application

open System
open System.Text.Json.Serialization

type TokenClaims = {
    [<JsonPropertyName "sub">] Subject: string
    [<JsonPropertyName "exp">] Expiry: int
}

module TokenClaims =
    let [<Literal>] LIFETIME_SECS = 60 * 60 * 24

    let create subjectId (currentTime: DateTime) = {
        Subject = subjectId
        Expiry = ((currentTime - DateTime.UnixEpoch).TotalSeconds |> int) + LIFETIME_SECS
    }

    let isActive (currentTime: DateTime) claims =
        claims.Expiry > ((currentTime - DateTime.UnixEpoch).TotalSeconds |> int)
