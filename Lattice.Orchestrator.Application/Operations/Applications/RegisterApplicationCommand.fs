namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

type RegisterApplicationCommandProps = {
    DiscordBotToken: string
}

type RegisterApplicationCommandError =
    | InvalidToken
    | RegistrationFailed

module RegisterApplicationCommand =
    let run (env: #IDiscord & #IPersistence & #ISecrets) (props: RegisterApplicationCommandProps) = task {
        // Validate application with Discord
        match! env.GetApplicationInformation props.DiscordBotToken with
        | None -> return Error RegisterApplicationCommandError.InvalidToken
        | Some discordApplication ->

        // Create application and save to db
        let encryptedBotToken = props.DiscordBotToken |> Aes.encrypt env.BotTokenEncryptionKey

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

        let application = Application.create discordApplication.Id encryptedBotToken privilegedIntents

        match! env.UpsertApplication application with
        | Error _ -> return Error RegisterApplicationCommandError.RegistrationFailed
        | Ok application -> return Ok application
    }
