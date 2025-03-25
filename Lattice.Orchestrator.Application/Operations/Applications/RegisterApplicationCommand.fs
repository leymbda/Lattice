namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type RegisterApplicationCommandProps = {
    UserId: string
    DiscordBotToken: string
}

type RegisterApplicationCommandError =
    | Forbidden
    | InvalidBotToken
    | RegistrationFailed

module RegisterApplicationCommand =
    let run (env: #ICache & #IDiscord & #IPersistence & #ISecrets) (props: RegisterApplicationCommandProps) = task {
        // Validate application with Discord
        match! env.GetApplicationInformation props.DiscordBotToken with
        | None -> return Error RegisterApplicationCommandError.InvalidBotToken
        | Some application ->

        // Ensure user has access to application
        let members = Map.empty<string, TeamMemberRole> // TODO: Map app owner/team-members into here (requires new FSharp.Discord version)
        let team = Team.create application.Id members // TODO: This is a duplicate of code in the Cache composite, this should all be refactored to be neat

        match team.Members.TryFind props.UserId with
        | None -> return Error RegisterApplicationCommandError.Forbidden
        | Some _ ->

        // Create application and save to db
        let encryptedBotToken = props.DiscordBotToken |> Aes.encrypt env.BotTokenEncryptionKey

        let privilegedIntents =
            {
                MessageContent = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_MESSAGE_CONTENT
                MessageContentLimited = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED
                GuildMembers = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_GUILD_MEMBERS
                GuildMembersLimited = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED
                Presence = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_PRESENCE
                PresenceLimited = application.Flags |> Option.defaultValue [] |> List.contains ApplicationFlag.GATEWAY_PRESENCE_LIMITED
            }

        let app = App.create application.Id encryptedBotToken privilegedIntents

        match! env.SetApp app with
        | Error _ -> return Error RegisterApplicationCommandError.RegistrationFailed
        | Ok app ->

        // Update team in cache
        do! env.SetTeam team :> Task
        return Ok app
    }
