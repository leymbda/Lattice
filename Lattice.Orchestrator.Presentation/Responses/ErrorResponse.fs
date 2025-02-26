namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type ErrorResponse = {
    Code: ErrorCode
    Message: string
}

module ErrorResponse =
    let encoder (v: ErrorResponse) =
        Encode.object [
            "code", Encode.Enum.int<ErrorCode> v.Code
            "message", Encode.string v.Message
        ]

    let fromCode code = {
        Code = code
        Message = ErrorCode.getMessage code
    }

    let fromSerializationError message = {
        Code = ErrorCode.MALFORMED_REQUEST_BODY
        Message = message
    }
