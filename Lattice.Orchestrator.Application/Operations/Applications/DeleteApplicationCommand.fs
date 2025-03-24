namespace Lattice.Orchestrator.Application

type DeleteApplicationCommandProps = {
    UserId: string
    ApplicationId: string
}

type DeleteApplicationCommandError =
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound

module DeleteApplicationCommand =
    let run (env: #IPersistence & #ISecrets) (props: DeleteApplicationCommandProps) = task {
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok application ->
        
        let decryptedBotToken = Aes.decrypt env.BotTokenEncryptionKey application.EncryptedBotToken
        
        // Ensure user has access to application
        match! TeamAdapter.getTeam env application.Id decryptedBotToken with
        | None -> return Error DeleteApplicationCommandError.TeamNotFound
        | Some team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error DeleteApplicationCommandError.Forbidden
        | Some _ -> 

        // Delete application
        match! env.DeleteApplicationById props.ApplicationId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok () -> return Ok ()
    }
