namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type SyncApplicationPrivilegedIntentsCommandProps = {
    ApplicationId: string
}

type SyncApplicationPrivilegedIntentsCommandError =
    | ApplicationNotFound
    | InvalidToken
    | DifferentBotToken
    | UpdateFailed

module SyncApplicationPrivilegedIntentsCommand =
    let run (env: #IDiscord & #IPersistence & #ISecrets) (props: SyncApplicationPrivilegedIntentsCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error SyncApplicationPrivilegedIntentsCommandError.ApplicationNotFound
        | Ok app ->

        let discordBotToken = app.EncryptedBotToken |> Aes.decrypt env.BotTokenEncryptionKey

        // Get the current privileged intents
        match! env.GetApplicationInformation discordBotToken with
        | None -> return Error SyncApplicationPrivilegedIntentsCommandError.InvalidToken
        | Some app when app.Id <> props.ApplicationId -> return Error SyncApplicationPrivilegedIntentsCommandError.DifferentBotToken
        | Some discordApp ->

        // Update privileged intents in db
        let privilegedIntents =
            {
                MessageContent = discordApp.HasMessageContentIntent
                MessageContentLimited = discordApp.HasMessageContentLimitedIntent
                GuildMembers = discordApp.HasGuildMembersIntent
                GuildMembersLimited = discordApp.HasGuildMembersLimitedIntent
                Presence = discordApp.HasPresenceIntent
                PresenceLimited = discordApp.HasPresenceLimitedIntent
            }

        let updatedApp = app |> Application.setPrivilegedIntents privilegedIntents

        match! env.UpsertApplication updatedApp with
        | Error _ -> return Error SyncApplicationPrivilegedIntentsCommandError.UpdateFailed
        | Ok app -> return Ok app.PrivilegedIntents
    }
