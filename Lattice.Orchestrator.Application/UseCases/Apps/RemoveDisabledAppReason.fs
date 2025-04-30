module Lattice.Orchestrator.Application.RemoveDisabledAppReason

open Lattice.Orchestrator.Domain

type Props = {
    AppId: string
    DisabledReason: DisabledAppReason
}

type Failure =
    | ApplicationNotFound
    | RemoveFailed

let run (env: #IPersistence) props = task {
    // Get current application from db
    match! env.GetApp props.AppId with
    | Error _ -> return Error ApplicationNotFound
    | Ok app ->

    // Remove handler from application
    let updatedApp = app |> App.removeDisabledReason props.DisabledReason

    match! env.SetApp updatedApp with
    | Ok app -> return Ok app.DisabledReasons
    | _ -> return Error RemoveFailed
}
