namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type AddDisabledApplicationReasonCommandProps = {
    ApplicationId: string
    DisabledReason: DisabledApplicationReason
}

type AddDisabledApplicationReasonCommandError =
    | ApplicationNotFound
    | AddFailed

module AddDisabledApplicationReasonCommand =
    let run (env: #IPersistence) (props: AddDisabledApplicationReasonCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error AddDisabledApplicationReasonCommandError.ApplicationNotFound
        | Ok app ->

        // Remove handler from application
        let updatedApp = app |> Application.addDisabledReason props.DisabledReason

        match! env.UpsertApplication updatedApp with
        | Ok app -> return Ok app.DisabledReasons
        | _ -> return Error AddDisabledApplicationReasonCommandError.AddFailed
    }
