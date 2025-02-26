namespace Lattice.Orchestrator.Presentation

type ErrorCode =
    | INTERNAL_SERVER_ERROR = 1000
    | MALFORMED_REQUEST_BODY = 1001
    | APPLICATION_NOT_FOUND = 2000
    | APPLICATION_NOT_ACTIVATED = 2001
    | NODE_NOT_FOUND = 2002
    | INVALID_TOKEN = 3000
    | DIFFERENT_BOT_TOKEN = 3001
    | INVALID_OAUTH_CODE = 3002

module ErrorCode =
    let getMessage (code: ErrorCode) =
        match code with
        | ErrorCode.INTERNAL_SERVER_ERROR -> "Internal server error"
        | ErrorCode.MALFORMED_REQUEST_BODY -> "Malformed request body"
        | ErrorCode.APPLICATION_NOT_FOUND -> "Application not found"
        | ErrorCode.APPLICATION_NOT_ACTIVATED -> "Application not activated"
        | ErrorCode.NODE_NOT_FOUND -> "Node not found"
        | ErrorCode.INVALID_TOKEN -> "Invalid Discord bot token provided"
        | ErrorCode.DIFFERENT_BOT_TOKEN -> "Provided Discord bot token is for a different application"
        | ErrorCode.INVALID_OAUTH_CODE -> "Invalid OAuth code provided"
        | _ -> "Unknown error"
