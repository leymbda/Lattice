namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type ErrorResponse = {
    Code: ErrorCode
    Message: string
}

module ErrorResponse =
    module Property =
        let [<Literal>] Code = "code"
        let [<Literal>] Message = "message"

    let decoder: Decoder<ErrorResponse> =
        Decode.object (fun get -> {
            Code = get.Required.Field Property.Code Decode.Enum.int<ErrorCode>
            Message = get.Required.Field Property.Message Decode.string
        })

    let encoder (v: ErrorResponse) =
        Encode.object [
            Property.Code, Encode.Enum.int<ErrorCode> v.Code
            Property.Message, Encode.string v.Message
        ]

    let fromCode code = {
        Code = code
        Message = ErrorCode.getMessage code
    }

    let fromSerializationError message = {
        Code = ErrorCode.MALFORMED_REQUEST_BODY
        Message = message
    }
