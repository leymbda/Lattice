namespace Lattice.Orchestrator.Contracts

type ErrorCode =
    | INTERNAL_SERVER_ERROR = 1000
    | MALFORMED_REQUEST_BODY = 1001
    | FORBIDDEN = 1002
    | APP_NOT_FOUND = 2000
    | APP_NOT_ACTIVATED = 2001
    | NODE_NOT_FOUND = 2002
    | TEAM_NOT_FOUND = 2003
    | INVALID_TOKEN = 3000
    | DIFFERENT_BOT_TOKEN = 3001

module ErrorCode =
    let getMessage (code: ErrorCode) =
        match code with
        | ErrorCode.INTERNAL_SERVER_ERROR -> "Internal server error"
        | ErrorCode.MALFORMED_REQUEST_BODY -> "Malformed request body"
        | ErrorCode.FORBIDDEN -> "You are not authorized to access this resource"
        | ErrorCode.APP_NOT_FOUND -> "Appl not found"
        | ErrorCode.APP_NOT_ACTIVATED -> "App not activated"
        | ErrorCode.NODE_NOT_FOUND -> "Node not found"
        | ErrorCode.TEAM_NOT_FOUND -> "Team not found"
        | ErrorCode.INVALID_TOKEN -> "Invalid Discord bot token provided"
        | ErrorCode.DIFFERENT_BOT_TOKEN -> "Provided Discord bot token is for a different app"
        | _ -> "Unknown error"
