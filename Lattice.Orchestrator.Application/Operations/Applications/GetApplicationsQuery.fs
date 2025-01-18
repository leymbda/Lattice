namespace Lattice.Orchestrator.Application

type GetApplicationsQueryProps = {
    Before: string option
    After: string option
    Limit: int option
}

type GetApplicationsQueryError =
    | InvalidToken

module GetApplicationsQuery =
    let run (env) (props: GetApplicationsQueryProps) = task {
        return Error GetApplicationsQueryError.InvalidToken
    }
