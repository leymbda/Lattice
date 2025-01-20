namespace Lattice.Orchestrator.Presentation

type ErrorCode = | ErrorCode // Placeholder for actual error codes (not currently needed

module ErrorCode =
    let getMessage (errorCode: ErrorCode) =
        match errorCode with
        | _ -> "Unknown error"

type ErrorResponseCode =
    | INVALID_TOKEN = 4000
    | APPLICATION_NOT_FOUND = 4001
    | INTERNAL_SERVER_ERROR = 5000

module ErrorResponseCode =
    let getMessage (errorCode: ErrorResponseCode) =
        match errorCode with
        | ErrorResponseCode.INVALID_TOKEN -> "Invalid Discord bot token provided"
        | ErrorResponseCode.APPLICATION_NOT_FOUND -> "Application not found"
        | ErrorResponseCode.INTERNAL_SERVER_ERROR -> "Internal server error"
        | _ -> "Unknown error"
