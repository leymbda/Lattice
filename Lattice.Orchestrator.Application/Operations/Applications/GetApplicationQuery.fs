namespace Lattice.Orchestrator.Application

type GetApplicationQueryProps = {
    ApplicationId: string
}

type GetApplicationQueryError =
    | ApplicationNotFound

module GetApplicationQuery =
    let run (env: #IPersistence) (props: GetApplicationQueryProps) = task {
        // Fetch application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error GetApplicationQueryError.ApplicationNotFound
        | Ok application -> return Ok application
    }
