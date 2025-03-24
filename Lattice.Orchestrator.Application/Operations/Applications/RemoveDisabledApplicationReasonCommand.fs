namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type RemoveDisabledApplicationReasonCommandProps = {
    AppId: string
    DisabledReason: DisabledApplicationReason
}

type RemoveDisabledApplicationReasonCommandError =
    | ApplicationNotFound
    | RemoveFailed

module RemoveDisabledApplicationReasonCommand =
    let run (env: #IPersistence) (props: RemoveDisabledApplicationReasonCommandProps) = task {
        // Get current application from db
        match! env.GetApp props.AppId with
        | Error _ -> return Error RemoveDisabledApplicationReasonCommandError.ApplicationNotFound
        | Ok app ->

        // Remove handler from application
        let updatedApp = app |> App.removeDisabledReason props.DisabledReason

        match! env.SetApp updatedApp with
        | Ok app -> return Ok app.DisabledReasons
        | _ -> return Error RemoveDisabledApplicationReasonCommandError.RemoveFailed
    }
