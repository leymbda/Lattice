module Lattice.Orchestrator.Application.SyncApplicationPrivilegedIntents

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

type Props = {
    UserId: string
    AppId: string
}

type Failure =
    | InvalidBotToken
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound
    | DifferentBotToken
    | UpdateFailed

let run (env: #IDiscord & #IPersistence & #ISecrets) props = task {
    // Get current application from db
    match! env.GetApp props.AppId with
    | Error _ -> return Error ApplicationNotFound
    | Ok app ->

    let discordBotToken = app.EncryptedBotToken |> Aes.decrypt env.BotTokenEncryptionKey

    // Ensure user has access to application
    match! TeamAdapter.getTeam env app.Id discordBotToken with
    | None -> return Error TeamNotFound
    | Some team ->

    match team.Members.TryFind props.UserId with
    | None -> return Error Forbidden
    | Some _ -> 

    // Get the current privileged intents
    match! env.GetApplicationInformation discordBotToken with
    | None -> return Error InvalidBotToken
    | Some app when app.Id <> app.Id -> return Error DifferentBotToken
    | Some application ->

    // Update privileged intents in db
    let privilegedIntents =
        application.Flags
        |> Option.defaultValue []
        |> PrivilegedIntents.fromFlags

    let updatedApp = app |> App.setPrivilegedIntents privilegedIntents

    match! env.SetApp updatedApp with
    | Error _ -> return Error UpdateFailed
    | Ok app -> return Ok app.PrivilegedIntents
}
