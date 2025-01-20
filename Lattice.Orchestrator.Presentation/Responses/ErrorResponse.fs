namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type Error = {
    [<JsonPropertyName "code">] Code: ErrorCode
    [<JsonPropertyName "message">] Message: string
}

module Error =
    let fromCode (code: ErrorCode) =
        { Code = code; Message = ErrorCode.getMessage code }

type ErrorResponse = {
    [<JsonPropertyName "code">] Code: ErrorResponseCode
    [<JsonPropertyName "message">] Message: string
    [<JsonPropertyName "errors">] Errors: Error list option
}

module ErrorResponse =
    let fromCode (code: ErrorResponseCode) =
        { Code = code; Message = ErrorResponseCode.getMessage code; Errors = None }

    let fromCodeWithErrors (code: ErrorResponseCode) (errors: Error list) =
        { Code = code; Message = ErrorResponseCode.getMessage code; Errors = Some errors }
