namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type GetApplicationQueryProps = {
    UserId: string
    ApplicationId: string
}

type GetApplicationQueryError =
    | Forbidden
    | ApplicationNotFound
    | InvalidBotToken

module GetApplicationQuery =
    let run (env: #IPersistence) (props: GetApplicationQueryProps) = task {
        // Fetch application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error GetApplicationQueryError.ApplicationNotFound
        | Ok application ->

        // Ensure user has access to application
        match! Cache.getTeam env application with
        | Error GetTeamError.InvalidBotToken -> return Error GetApplicationQueryError.InvalidBotToken
        | Ok team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error GetApplicationQueryError.Forbidden
        | Some _ -> return Ok application
    }
