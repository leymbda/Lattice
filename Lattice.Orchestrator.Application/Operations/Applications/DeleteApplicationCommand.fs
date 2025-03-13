namespace Lattice.Orchestrator.Application

type DeleteApplicationCommandProps = {
    UserId: string
    ApplicationId: string
}

type DeleteApplicationCommandError =
    | Forbidden
    | ApplicationNotFound

module DeleteApplicationCommand =
    let run (env: #IPersistence) (props: DeleteApplicationCommandProps) = task {
        // TODO: Check if user is authorized to handle this application

        match! env.DeleteApplicationById props.ApplicationId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok () -> return Ok ()
    }
