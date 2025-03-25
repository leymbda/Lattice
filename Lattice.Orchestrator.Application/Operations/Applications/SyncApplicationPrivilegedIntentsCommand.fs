namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

type SyncApplicationPrivilegedIntentsCommandProps = {
    UserId: string
    AppId: string
}

type SyncApplicationPrivilegedIntentsCommandError =
    | InvalidBotToken
    | Forbidden
    | ApplicationNotFound
    | TeamNotFound
    | DifferentBotToken
    | UpdateFailed

module SyncApplicationPrivilegedIntentsCommand =
    let run (env: #IDiscord & #IPersistence & #ISecrets) (props: SyncApplicationPrivilegedIntentsCommandProps) = task {
        // Get current application from db
        match! env.GetApp props.AppId with
        | Error _ -> return Error SyncApplicationPrivilegedIntentsCommandError.ApplicationNotFound
        | Ok app ->

        let discordBotToken = app.EncryptedBotToken |> Aes.decrypt env.BotTokenEncryptionKey

        // Ensure user has access to application
        match! TeamAdapter.getTeam env app.Id discordBotToken with
        | None -> return Error SyncApplicationPrivilegedIntentsCommandError.TeamNotFound
        | Some team ->

        match team.Members.TryFind props.UserId with
        | None -> return Error SyncApplicationPrivilegedIntentsCommandError.Forbidden
        | Some _ -> 

        // Get the current privileged intents
        match! env.GetApplicationInformation discordBotToken with
        | None -> return Error SyncApplicationPrivilegedIntentsCommandError.InvalidBotToken
        | Some app when app.Id <> app.Id -> return Error SyncApplicationPrivilegedIntentsCommandError.DifferentBotToken
        | Some application ->

        // Update privileged intents in db
        let privilegedIntents =
            {
                MessageContent = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_MESSAGE_CONTENT
                MessageContentLimited = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED
                GuildMembers = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_GUILD_MEMBERS
                GuildMembersLimited = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED
                Presence = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_PRESENCE
                PresenceLimited = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_PRESENCE_LIMITED
            }

        let updatedApp = app |> App.setPrivilegedIntents privilegedIntents

        match! env.SetApp updatedApp with
        | Error _ -> return Error SyncApplicationPrivilegedIntentsCommandError.UpdateFailed
        | Ok app -> return Ok app.PrivilegedIntents
    }
