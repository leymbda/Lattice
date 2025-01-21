namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type RegisterApplicationCommandProps = {
    DiscordBotToken: string
}

type RegisterApplicationCommandError =
    | InvalidToken
    | RegistrationFailed

module RegisterApplicationCommand =
    let run (env: #IDiscord & #IPersistence) (props: RegisterApplicationCommandProps) = task {
        // Validate application with Discord
        match! env.GetApplicationInformation props.DiscordBotToken with
        | None -> return Error RegisterApplicationCommandError.InvalidToken
        | Some discordApplication ->

        // Create application and save to db
        let privilegedIntents =
            {
                MessageContent = discordApplication.HasMessageContentIntent
                MessageContentLimited = discordApplication.HasMessageContentLimitedIntent
                GuildMembers = discordApplication.HasGuildMembersIntent
                GuildMembersLimited = discordApplication.HasGuildMembersLimitedIntent
                Presence = discordApplication.HasPresenceIntent
                PresenceLimited = discordApplication.HasPresenceLimitedIntent
            }

        let application = Application.create discordApplication.Id props.DiscordBotToken privilegedIntents

        match! env.UpsertApplication application with
        | Error _ -> return Error RegisterApplicationCommandError.RegistrationFailed
        | Ok application -> return Ok application
    }
