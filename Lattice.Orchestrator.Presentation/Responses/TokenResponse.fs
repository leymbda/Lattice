namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type TokenResponse (token) =
    [<JsonPropertyName "token">]
    member _.Token: string = token
