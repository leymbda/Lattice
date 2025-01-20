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
        let application =
            Application.register discordApplication.Id props.DiscordBotToken
            |> Application.REGISTERED

        match! env.UpsertApplication application with
        | Error _ -> return Error RegisterApplicationCommandError.RegistrationFailed
        | Ok application -> return Ok application
    }
