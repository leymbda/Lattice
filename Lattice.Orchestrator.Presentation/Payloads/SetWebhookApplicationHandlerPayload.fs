namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type SetWebhookApplicationHandlerPayload (endpoint) =
    [<JsonPropertyName "endpoint">]
    member _.Endpoint: string = endpoint
