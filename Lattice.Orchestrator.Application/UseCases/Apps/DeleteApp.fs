module Lattice.Orchestrator.Application.DeleteApp

type Props = {
    UserId: string
    AppId: string
}

type Failure =
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound

let run (env: #IPersistence & #ISecrets) props = task {
    match! env.GetApp props.AppId with
    | Error _ -> return Error ApplicationNotFound
    | Ok app ->
        
    let decryptedBotToken = Aes.decrypt env.BotTokenEncryptionKey app.EncryptedBotToken
        
    // Ensure user has access to application
    match! TeamAdapter.getTeam env app.Id decryptedBotToken with
    | None -> return Error TeamNotFound
    | Some team ->

    match team.Members.TryFind props.UserId with
    | None -> return Error Forbidden
    | Some _ -> 

    // Delete application
    match! env.RemoveApp props.AppId with
    | Error _ -> return Error ApplicationNotFound
    | Ok () -> return Ok ()
}
