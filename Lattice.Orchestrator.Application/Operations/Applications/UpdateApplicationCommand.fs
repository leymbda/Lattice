namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type UpdateApplicationCommandHandlerProps =
    | WEBHOOK of endpoint: string
    | SERVICE_BUS of queueName: string * connectionString: string

type UpdateApplicationCommandProps = {
    UserId: string
    ApplicationId: string
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    Handler: UpdateApplicationCommandHandlerProps option option
}

type UpdateApplicationCommandError =
    | InvalidBotToken
    | Forbidden
    | ApplicationNotFound
    | DifferentBotToken
    | UpdateFailed

module UpdateApplicationCommand =
    let run (env: #IDiscord & #IPersistence & #ISecrets) (props: UpdateApplicationCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error UpdateApplicationCommandError.ApplicationNotFound
        | Ok app ->

        // Ensure user has access to application
        match! Cache.getTeam env app with
        | Error GetTeamError.InvalidBotToken -> return Error UpdateApplicationCommandError.InvalidBotToken
        | Ok team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error UpdateApplicationCommandError.Forbidden
        | Some _ -> 

        // Ensure the new bot token if valid if one is provided
        let! error =
            match props.DiscordBotToken with
            | None -> Task.FromResult None
            | Some discordBotToken -> task {
                match! env.GetApplicationInformation discordBotToken with
                | None -> return Some UpdateApplicationCommandError.InvalidBotToken
                | Some app when app.Id <> props.ApplicationId -> return Some UpdateApplicationCommandError.DifferentBotToken
                | Some _ -> return None
            }

        match error with
        | Some err -> return Error err
        | None ->

        // Configure or remove handler based on prop
        let updateHandler handler app =
            match handler with
            | Some (UpdateApplicationCommandHandlerProps.WEBHOOK endpoint) ->
                let ed25519 = Ed25519.generate()
                let handler = Handler.WEBHOOK (WebhookHandler.create endpoint ed25519.PublicKey ed25519.PrivateKey)
                app |> Application.setHandler handler

            | Some (UpdateApplicationCommandHandlerProps.SERVICE_BUS (queueName, connectionString)) ->
                let handler = Handler.SERVICE_BUS (ServiceBusHandler.create queueName connectionString)
                app |> Application.setHandler handler

            | None ->
                app |> Application.removeHandler

        // Update provided properties
        let encryptedBotToken = props.DiscordBotToken |> Option.map (Aes.encrypt env.BotTokenEncryptionKey)

        let updatedApp =
            app
            |> Option.foldBack (fun encryptedBotToken app -> Application.setEncryptedBotToken encryptedBotToken app) encryptedBotToken
            |> Option.foldBack (fun intents app -> Application.setIntents intents app) props.Intents
            |> Option.foldBack (fun shardCount app -> Application.setShardCount shardCount app) props.ShardCount
            |> Option.foldBack (fun handler app -> app |> updateHandler handler) props.Handler

        match! env.UpsertApplication updatedApp with
        | Error _ -> return Error UpdateApplicationCommandError.UpdateFailed
        | Ok app -> return Ok app
    }
