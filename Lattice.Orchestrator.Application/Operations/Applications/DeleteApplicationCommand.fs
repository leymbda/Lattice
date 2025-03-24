namespace Lattice.Orchestrator.Application

type DeleteApplicationCommandProps = {
    UserId: string
    AppId: string
}

type DeleteApplicationCommandError =
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound

module DeleteApplicationCommand =
    let run (env: #IPersistence & #ISecrets) (props: DeleteApplicationCommandProps) = task {
        match! env.GetApp props.AppId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok app ->
        
        let decryptedBotToken = Aes.decrypt env.BotTokenEncryptionKey app.EncryptedBotToken
        
        // Ensure user has access to application
        match! TeamAdapter.getTeam env app.Id decryptedBotToken with
        | None -> return Error DeleteApplicationCommandError.TeamNotFound
        | Some team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error DeleteApplicationCommandError.Forbidden
        | Some _ -> 

        // Delete application
        match! env.RemoveApp props.AppId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok () -> return Ok ()
    }
