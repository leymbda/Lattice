namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type GetApplicationQueryProps = {
    UserId: string
    ApplicationId: string
}

type GetApplicationQueryError =
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound

module GetApplicationQuery =
    let run (env: #IPersistence & #ISecrets) (props: GetApplicationQueryProps) = task {
        // Fetch application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error GetApplicationQueryError.ApplicationNotFound
        | Ok application ->

        let decryptedBotToken = Aes.decrypt env.BotTokenEncryptionKey application.EncryptedBotToken

        // Ensure user has access to application
        match! TeamAdapter.getTeam env application.Id decryptedBotToken with
        | None -> return Error GetApplicationQueryError.TeamNotFound
        | Some team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error GetApplicationQueryError.Forbidden
        | Some _ -> return Ok application
    }
