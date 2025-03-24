namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type GetApplicationQueryProps = {
    UserId: string
    AppId: string
}

type GetApplicationQueryError =
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound

module GetApplicationQuery =
    let run (env: #IPersistence & #ISecrets) (props: GetApplicationQueryProps) = task {
        // Fetch application from db
        match! env.GetApp props.AppId with
        | Error _ -> return Error GetApplicationQueryError.ApplicationNotFound
        | Ok app ->

        let decryptedBotToken = Aes.decrypt env.BotTokenEncryptionKey app.EncryptedBotToken

        // Ensure user has access to application
        match! TeamAdapter.getTeam env app.Id decryptedBotToken with
        | None -> return Error GetApplicationQueryError.TeamNotFound
        | Some team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error GetApplicationQueryError.Forbidden
        | Some _ -> return Ok app
    }
