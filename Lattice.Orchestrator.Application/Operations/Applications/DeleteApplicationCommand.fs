namespace Lattice.Orchestrator.Application

type DeleteApplicationCommandProps = {
    ApplicationId: string
}

type DeleteApplicationCommandError =
    | ApplicationNotFound

module DeleteApplicationCommand =
    let run (env: #IPersistence) (props: DeleteApplicationCommandProps) = task {
        match! env.DeleteApplicationById props.ApplicationId with
        | Error _ -> return Error DeleteApplicationCommandError.ApplicationNotFound
        | Ok () -> return Ok ()
    }
