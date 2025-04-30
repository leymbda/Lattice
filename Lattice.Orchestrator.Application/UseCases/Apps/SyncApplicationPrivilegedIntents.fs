module Lattice.Orchestrator.Application.SyncApplicationPrivilegedIntents

open FsToolkit.ErrorHandling
open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

type Props = {
    UserId: string
    AppId: string
}

type Failure =
    | InvalidBotToken
    | Forbidden
    | AppNotFound
    | TeamNotFound
    | DifferentBotToken
    | UpdateFailed

let run (env: #IDiscord & #IPersistence & #ISecrets) props = asyncResult {
    // Fetch app from db
    let! app =
        env.GetApp props.AppId
        |> Async.AwaitTask
        |> AsyncResult.setError AppNotFound
        
    let decryptedBotToken =
        app.EncryptedBotToken
        |> Aes.decrypt env.BotTokenEncryptionKey

    // Ensure user has access to app
    do!
        TeamAdapter.getTeam env app.Id decryptedBotToken
        |> Async.AwaitTask
        |> AsyncResult.requireSome TeamNotFound
        |> AsyncResult.map (_.Members.ContainsKey(props.UserId))
        |> AsyncResult.bindRequireTrue Forbidden
        
    // Get the current privileged intents
    let! application =
        env.GetApplicationInformation decryptedBotToken
        |> Async.AwaitTask
        |> AsyncResult.requireSome InvalidBotToken

    do!
        application.Id = app.Id
        |> Result.requireTrue DifferentBotToken

    // Update privileged intents in db
    let privilegedIntents =
        application.Flags
        |> Option.defaultValue []
        |> PrivilegedIntents.fromFlags

    return!
        app
        |> App.setPrivilegedIntents privilegedIntents
        |> env.SetApp
        |> Async.AwaitTask
        |> AsyncResult.setError UpdateFailed
        |> AsyncResult.map _.PrivilegedIntents
}
