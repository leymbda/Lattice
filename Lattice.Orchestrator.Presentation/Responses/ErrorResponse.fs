namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type ErrorResponse (code, message) =
    [<JsonPropertyName "code">]
    member _.Code: ErrorCode = code
    
    [<JsonPropertyName "message">]
    member _.Message: string = message

module ErrorResponse =
    let fromCode (code: ErrorCode) =
        ErrorResponse(code, ErrorCode.getMessage code)
