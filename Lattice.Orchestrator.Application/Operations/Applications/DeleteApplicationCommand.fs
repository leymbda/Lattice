namespace Lattice.Orchestrator.Application

type DeleteApplicationCommandProps = {
    UserId: string
    ApplicationId: string
}

type DeleteApplicationCommandError =
    | InvalidBotToken
    | Forbidden
    | ApplicationNotFound

module DeleteApplicationCommand =
    let run (env: #IPersistence) (props: DeleteApplicationCommandProps) = task {
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok application ->
        
        // Ensure user has access to application
        match! Cache.getTeam env application with
        | Error GetTeamError.InvalidBotToken -> return Error DeleteApplicationCommandError.InvalidBotToken
        | Ok team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error DeleteApplicationCommandError.Forbidden
        | Some _ -> 

        // Delete application
        match! env.DeleteApplicationById props.ApplicationId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok () -> return Ok ()
    }
