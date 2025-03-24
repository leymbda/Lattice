namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

type RegisterApplicationCommandProps = {
    UserId: string
    DiscordBotToken: string
}

type RegisterApplicationCommandError =
    | Forbidden
    | InvalidBotToken
    | RegistrationFailed

module RegisterApplicationCommand =
    let run (env: #IDiscord & #IPersistence & #ISecrets) (props: RegisterApplicationCommandProps) = task {
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

        let hasFlag (flag: ApplicationFlag) (app: FSharp.Discord.Types.Application) =
            (Option.defaultValue 0 app.Flags &&& int flag) = int flag

        let privilegedIntents =
            {
                MessageContent = application |> hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT
                MessageContentLimited = application |> hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED
                GuildMembers = application |> hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS
                GuildMembersLimited = application |> hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED
                Presence = application |> hasFlag ApplicationFlag.GATEWAY_PRESENCE
                PresenceLimited = application |> hasFlag ApplicationFlag.GATEWAY_PRESENCE_LIMITED
            }

        // TODO: Update `Application` object in FSharp.Discord to return a list of flags to check if contained rather than defining this custom function as above

        let app = App.create application.Id encryptedBotToken privilegedIntents

        match! env.SetApp app with
        | Error _ -> return Error RegisterApplicationCommandError.RegistrationFailed
        | Ok app -> return Ok app

        // TODO: Update team cache here (does not matter if it succeeds or fails)
    }
