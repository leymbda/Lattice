namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type LoginPayload (code, redirectUri) =
    [<JsonPropertyName "code">]
    member _.Code: string = code
    
    [<JsonPropertyName "redirectUri">]
    member _.RedirectUri: string = redirectUri
