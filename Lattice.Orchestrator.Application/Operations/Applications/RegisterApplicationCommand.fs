namespace Lattice.Orchestrator.Application

type RegisterApplicationCommandProps = {
    DiscordBotToken: string
}

type RegisterApplicationCommandError =
    | InvalidToken

module RegisterApplicationCommand =
    let run (env: #IDiscord) (props: RegisterApplicationCommandProps) = task {
        match! env.GetApplicationInformation props.DiscordBotToken with
        | None -> return Error RegisterApplicationCommandError.InvalidToken
        | Some app -> return Ok ()

        // TODO: Properly implement (currently just for testing DI)
    }
