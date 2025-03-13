namespace Lattice.Orchestrator.Application

type GetApplicationQueryProps = {
    UserId: string
    ApplicationId: string
}

type GetApplicationQueryError =
    | Forbidden
    | ApplicationNotFound

module GetApplicationQuery =
    let run (env: #IPersistence) (props: GetApplicationQueryProps) = task {
        // Fetch application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error GetApplicationQueryError.ApplicationNotFound
        | Ok application -> return Ok application

        // TODO: Check if user is authorized to handle this application
    }
