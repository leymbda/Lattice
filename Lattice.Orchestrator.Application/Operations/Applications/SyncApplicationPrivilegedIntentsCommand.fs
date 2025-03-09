namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

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
        | Some discordApplication ->

        // Update privileged intents in db
        let hasFlag (flag: ApplicationFlag) (app: FSharp.Discord.Types.Application) =
            (Option.defaultValue 0 app.Flags &&& int flag) = int flag

        let privilegedIntents =
            {
                MessageContent = discordApplication |> hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT
                MessageContentLimited = discordApplication |> hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED
                GuildMembers = discordApplication |> hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS
                GuildMembersLimited = discordApplication |> hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED
                Presence = discordApplication |> hasFlag ApplicationFlag.GATEWAY_PRESENCE
                PresenceLimited = discordApplication |> hasFlag ApplicationFlag.GATEWAY_PRESENCE_LIMITED
            }

        // TODO: Update `Application` object in FSharp.Discord to return a list of flags to check if contained rather than defining this custom function as above

        let updatedApp = app |> Application.setPrivilegedIntents privilegedIntents

        match! env.UpsertApplication updatedApp with
        | Error _ -> return Error SyncApplicationPrivilegedIntentsCommandError.UpdateFailed
        | Ok app -> return Ok app.PrivilegedIntents
    }
