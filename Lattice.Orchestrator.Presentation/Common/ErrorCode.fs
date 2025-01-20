namespace Lattice.Orchestrator.Presentation

type ErrorCode =
    | INTERNAL_SERVER_ERROR = 1000
    | APPLICATION_NOT_FOUND = 2000
    | APPLICATION_NOT_ACTIVATED = 2001
    | INVALID_TOKEN = 3000
    | DIFFERENT_BOT_TOKEN = 3001

module ErrorCode =
    let getMessage (code: ErrorCode) =
        match code with
        | ErrorCode.INTERNAL_SERVER_ERROR -> "Internal server error"
        | ErrorCode.APPLICATION_NOT_FOUND -> "Application not found"
        | ErrorCode.APPLICATION_NOT_ACTIVATED -> "Application not activated"
        | ErrorCode.INVALID_TOKEN -> "Invalid Discord bot token provided"
        | ErrorCode.DIFFERENT_BOT_TOKEN -> "Provided Discord bot token is for a different application"
        | _ -> "Unknown error"
