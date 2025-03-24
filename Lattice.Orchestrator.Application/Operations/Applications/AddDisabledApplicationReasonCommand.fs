namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type AddDisabledApplicationReasonCommandProps = {
    AppId: string
    DisabledReason: DisabledApplicationReason
}

type AddDisabledApplicationReasonCommandError =
    | ApplicationNotFound
    | AddFailed

module AddDisabledApplicationReasonCommand =
    let run (env: #IPersistence) (props: AddDisabledApplicationReasonCommandProps) = task {
        // Get current application from db
        match! env.GetApp props.AppId with
        | Error _ -> return Error AddDisabledApplicationReasonCommandError.ApplicationNotFound
        | Ok app ->

        // Remove handler from application
        let updatedApp = app |> App.addDisabledReason props.DisabledReason

        match! env.SetApp updatedApp with
        | Ok app -> return Ok app.DisabledReasons
        | _ -> return Error AddDisabledApplicationReasonCommandError.AddFailed
    }
