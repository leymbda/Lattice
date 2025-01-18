namespace Lattice.Orchestrator.Application

type RemoveApplicationHandlerCommandProps = {
    ApplicationId: string
}

type RemoveApplicationHandlerCommandError =
    | InvalidToken

module RemoveApplicationHandlerCommand =
    let run (env) (props: RemoveApplicationHandlerCommandProps) = task {
        return Error RemoveApplicationHandlerCommandError.InvalidToken
    }
