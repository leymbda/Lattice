module Lattice.Orchestrator.Application.GetApp

open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Domain

type Props = {
    UserId: string
    AppId: string
}

type Failure =
    | Forbidden
    | AppNotFound
    | TeamNotFound

let run (env: #IPersistence & #ISecrets) props = asyncResult {
    // Fetch app from db
    let! app =
        env.GetApp props.AppId
        |> Async.AwaitTask
        |> AsyncResult.setError AppNotFound

    let decryptedBotToken =
        Aes.decrypt env.BotTokenEncryptionKey app.EncryptedBotToken

    // Ensure user has access to app
    do!
        TeamAdapter.getTeam env app.Id decryptedBotToken
        |> Async.AwaitTask
        |> AsyncResult.requireSome TeamNotFound
        |> AsyncResult.map (_.Members.ContainsKey(props.UserId))
        |> AsyncResult.bindRequireTrue Forbidden

    // Return app on success
    return app
}
