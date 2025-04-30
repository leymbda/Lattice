module Lattice.Orchestrator.Application.UpdateApp

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
    | ApplicationNotFound
    | TeamNotFound
    | InvalidBotToken
    | DifferentBotToken
    | UpdateFailed

let run (env: #IDiscord & #IPersistence & #ISecrets) props = task {
    // Get current application from db
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

    // Ensure the new bot token if valid if one is provided
    let! error =
        match props.DiscordBotToken with
        | None -> Task.FromResult None
        | Some discordBotToken -> task {
            match! env.GetApplicationInformation discordBotToken with
            | None -> return Some InvalidBotToken
            | Some application when application.Id <> app.Id -> return Some DifferentBotToken
            | Some _ -> return None
        }

    match error with
    | Some err -> return Error err
    | None ->

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
    let encryptedBotToken = props.DiscordBotToken |> Option.map (Aes.encrypt env.BotTokenEncryptionKey)

    let updatedApp =
        app
        |> Option.foldBack (fun encryptedBotToken app -> App.setEncryptedBotToken encryptedBotToken app) encryptedBotToken
        |> Option.foldBack (fun intents app -> App.setIntents intents app) props.Intents
        |> Option.foldBack (fun shardCount app -> App.setShardCount shardCount app) props.ShardCount
        |> Option.foldBack (fun handler app -> app |> updateHandler handler) props.Handler

    match! env.SetApp updatedApp with
    | Error _ -> return Error UpdateFailed
    | Ok app -> return Ok app
}
