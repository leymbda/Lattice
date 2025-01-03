namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

// TODO: Create enum for potential error codes

type Error = {
    [<JsonPropertyName "code">] Code: string
    [<JsonPropertyName "message">] Message: string
}

type ErrorResponse = {
    [<JsonPropertyName "code">] Code: string
    [<JsonPropertyName "message">] Message: string
    [<JsonPropertyName "errors">] Errors: Error list option
}
