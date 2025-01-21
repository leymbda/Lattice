namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type RemoveApplicationHandlerCommandProps = {
    ApplicationId: string
}

type RemoveApplicationHandlerCommandError =
    | ApplicationNotFound
    | RemovalFailed

module RemoveApplicationHandlerCommand =
    let run (env: #IPersistence) (props: RemoveApplicationHandlerCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error RemoveApplicationHandlerCommandError.ApplicationNotFound
        | Ok app ->

        // Remove handler from application
        let updatedApp = app |> Application.removeHandler

        match! env.UpsertApplication updatedApp with
        | Ok { Handler = None } -> return Ok ()
        | _ -> return Error RemoveApplicationHandlerCommandError.RemovalFailed
    }
