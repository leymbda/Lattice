module Lattice.Orchestrator.Application.UpdateApp

open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type HandlerProps =
    | WEBHOOK of endpoint: string
    | SERVICE_BUS of queueName: string * connectionString: string

type Props = {
    UserId: string
    AppId: string
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    Handler: HandlerProps option option
}

type Failure =
    | Forbidden
    | AppNotFound
    | TeamNotFound
    | InvalidBotToken
    | DifferentBotToken
    | UpdateFailed

let run (env: #IDiscord & #IPersistence & #ISecrets) props = asyncResult {
    // Get current app from db
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

    // Ensure the new bot token if valid if one is provided
    do!
        match props.DiscordBotToken with
        | None -> Task.FromResult (Ok ())
        | Some discordBotToken -> task {
            match! env.GetApplicationInformation discordBotToken with
            | None -> return Error InvalidBotToken
            | Some application when application.Id <> app.Id -> return Error DifferentBotToken
            | Some _ -> return Ok ()
        }
    
    // Configure or remove handler based on prop
    let updateHandler handler app =
        match handler with
        | Some (WEBHOOK endpoint) ->
            let ed25519 = Ed25519.generate()
            let handler = Handler.WEBHOOK (WebhookHandler.create endpoint ed25519.PublicKey ed25519.PrivateKey)
            app |> App.setHandler handler

        | Some (SERVICE_BUS (queueName, connectionString)) ->
            let handler = Handler.SERVICE_BUS (ServiceBusHandler.create queueName connectionString)
            app |> App.setHandler handler

        | None ->
            app |> App.removeHandler

    // Update provided properties
    let encryptedBotToken =
        props.DiscordBotToken
        |> Option.map (Aes.encrypt env.BotTokenEncryptionKey)

    return!
        app
        |> Option.foldBack (fun encryptedBotToken app -> App.setEncryptedBotToken encryptedBotToken app) encryptedBotToken
        |> Option.foldBack (fun intents app -> App.setIntents intents app) props.Intents
        |> Option.foldBack (fun shardCount app -> App.setShardCount shardCount app) props.ShardCount
        |> Option.foldBack (fun handler app -> app |> updateHandler handler) props.Handler
        |> env.SetApp
        |> Async.AwaitTask
        |> AsyncResult.setError UpdateFailed
}
