namespace Lattice.Orchestrator.Application

type UpdateApplicationCommandProps = {
    ApplicationId: string
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    DisabledReasons: int option
}

type UpdateApplicationCommandError =
    | InvalidToken

module UpdateApplicationCommand =
    let run (env) (props: UpdateApplicationCommandProps) = task {
        return Error UpdateApplicationCommandError.InvalidToken
    }
