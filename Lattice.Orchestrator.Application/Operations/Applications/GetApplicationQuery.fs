namespace Lattice.Orchestrator.Application

open FsToolkit.ErrorHandling
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
    let run (env: #IPersistence & #ISecrets) props = asyncResult {
        // Fetch application from db
        let! app =
            env.GetApp props.AppId
            |> Async.AwaitTask
            |> AsyncResult.setError ApplicationNotFound

        let decryptedBotToken =
            Aes.decrypt env.BotTokenEncryptionKey app.EncryptedBotToken

        // Ensure user has access to application
        do!
            TeamAdapter.getTeam env app.Id decryptedBotToken
            |> Async.AwaitTask
            |> AsyncResult.requireSome TeamNotFound
            |> AsyncResult.map (_.Members.ContainsKey(props.UserId))
            |> AsyncResult.bindRequireTrue Forbidden

        // Return app on success
        return app
    }
