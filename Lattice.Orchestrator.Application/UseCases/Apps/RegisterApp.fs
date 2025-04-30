module Lattice.Orchestrator.Application.RegisterApp

open FSharp.Discord.Types
open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type Props = {
    UserId: string
    DiscordBotToken: string
}

type Failure =
    | Forbidden
    | InvalidBotToken
    | RegistrationFailed

let run (env: #ICache & #IDiscord & #IPersistence & #ISecrets) props = asyncResult {
    // Validate app with Discord
    let! application =
        env.GetApplicationInformation props.DiscordBotToken
        |> Async.AwaitTask
        |> AsyncResult.requireSome InvalidBotToken
            
    // Ensure user has access to app
    let! team =
        Team.fromApplication application
        |> Result.requireSome RegistrationFailed

    do!
        team.Members.ContainsKey props.UserId
        |> Result.requireTrue Forbidden

    // Create app and save to db
    let encryptedBotToken =
        props.DiscordBotToken
        |> Aes.encrypt env.BotTokenEncryptionKey

    let privilegedIntents =
        application.Flags
        |> Option.defaultValue []
        |> PrivilegedIntents.fromFlags

    let! app =
        App.create application.Id encryptedBotToken privilegedIntents
        |> env.SetApp
        |> Async.AwaitTask
        |> AsyncResult.setError RegistrationFailed

    // Save team to cache
    do!
        env.SetTeam team
        |> Task.wait

    // Return newly registered app
    return app
}
