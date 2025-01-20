namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type UpdateApplicationCommandProps = {
    ApplicationId: string
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    DisabledReasons: int option
}

type UpdateApplicationCommandError =
    | ApplicationNotFound
    | InvalidToken
    | DifferentBotToken
    | UpdateFailed

module UpdateApplicationCommand =
    let run (env: #IDiscord & #IPersistence) (props: UpdateApplicationCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error UpdateApplicationCommandError.ApplicationNotFound
        | Ok app ->

        // Ensure the new bot token if valid if one is provided
        let! error =
            match props.DiscordBotToken with
            | None -> Task.FromResult None
            | Some discordBotToken -> task {
                match! env.GetApplicationInformation discordBotToken with
                | None -> return Some UpdateApplicationCommandError.InvalidToken
                | Some app when app.Id <> props.ApplicationId -> return Some UpdateApplicationCommandError.DifferentBotToken
                | Some _ -> return None
            }

        match error with
        | Some err -> return Error err
        | None ->

        // TODO: Reject request if intents or shard count provided when updating a registered application (maybe active with `None` handler if both provided?)

        // Update provided properties
        let updatedApp =
            app
            |> Option.foldBack (fun discordBotToken app -> Application.setDiscordBotToken discordBotToken app) props.DiscordBotToken
            |> Option.foldBack (fun disabledReasons app -> Application.addDisabledReason disabledReasons app) props.DisabledReasons
            |> (
                function
                | Application.ACTIVATED activatedApp ->
                    activatedApp
                    |> Option.foldBack (fun intents app -> Application.setIntents intents app) props.Intents
                    |> Option.foldBack (fun shardCount app -> Application.setProvisionedShardCount shardCount app) props.ShardCount
                    |> Application.ACTIVATED
                | app -> app
            )

        match! env.UpsertApplication updatedApp with
        | Error _ -> return Error UpdateApplicationCommandError.UpdateFailed
        | Ok app -> return Ok app
    }
