namespace Lattice.Orchestrator.Application

type GetApplicationQueryProps = {
    ApplicationId: string
}

type GetApplicationQueryError =
    | InvalidToken

module GetApplicationQuery =
    let run (env) (props: GetApplicationQueryProps) = task {
        return Error GetApplicationQueryError.InvalidToken
    }
