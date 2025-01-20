namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type ErrorResponse = {
    [<JsonPropertyName "code">] Code: ErrorCode
    [<JsonPropertyName "message">] Message: string
}

module ErrorResponse =
    let fromCode (code: ErrorCode) =
        { Code = code; Message = ErrorCode.getMessage code }
