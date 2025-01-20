namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type RemoveApplicationHandlerCommandProps = {
    ApplicationId: string
}

type RemoveApplicationHandlerCommandError =
    | ApplicationNotFound
    | ApplicationNotActivated
    | RemovalFailed

module RemoveApplicationHandlerCommand =
    let run (env: #IPersistence) (props: RemoveApplicationHandlerCommandProps) = task {
        // Get current application from db and ensure it is activated
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error RemoveApplicationHandlerCommandError.ApplicationNotFound
        | Ok (Application.REGISTERED _) -> return Error RemoveApplicationHandlerCommandError.ApplicationNotActivated
        | Ok (Application.ACTIVATED app) ->

        // Remove handler from application
        let updatedApp =
            app
            |> Application.removeHandler
            |> Application.ACTIVATED

        match! env.UpsertApplication updatedApp with
        | Ok (Application.ACTIVATED { Handler = None }) -> return Ok ()
        | _ -> return Error RemoveApplicationHandlerCommandError.RemovalFailed
    }
