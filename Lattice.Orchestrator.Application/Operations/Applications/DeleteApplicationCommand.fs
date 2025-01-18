namespace Lattice.Orchestrator.Application

type DeleteApplicationCommandProps = {
    ApplicationId: string
}

type DeleteApplicationCommandError =
    | InvalidToken

module DeleteApplicationCommand =
    let run (env) (props: DeleteApplicationCommandProps) = task {
        return Error DeleteApplicationCommandError.InvalidToken
    }
