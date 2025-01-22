namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type RemoveDisabledApplicationReasonCommandProps = {
    ApplicationId: string
    DisabledReason: DisabledApplicationReason
}

type RemoveDisabledApplicationReasonCommandError =
    | ApplicationNotFound
    | RemoveFailed

module RemoveDisabledApplicationReasonCommand =
    let run (env: #IPersistence) (props: RemoveDisabledApplicationReasonCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error RemoveDisabledApplicationReasonCommandError.ApplicationNotFound
        | Ok app ->

        // Remove handler from application
        let updatedApp = app |> Application.removeDisabledReason props.DisabledReason

        match! env.UpsertApplication updatedApp with
        | Ok app -> return Ok app.DisabledReasons
        | _ -> return Error RemoveDisabledApplicationReasonCommandError.RemoveFailed
    }
