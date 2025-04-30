module Lattice.Orchestrator.Application.AddDisabledAppReason

open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Domain

type Props = {
    AppId: string
    DisabledReason: DisabledAppReason
}

type Failure =
    | AppNotFound
    | AddFailed

let run (env: #IPersistence) props = asyncResult {
    // Fetch app from db
    let! app =
        env.GetApp props.AppId
        |> Async.AwaitTask
        |> AsyncResult.setError AppNotFound
        
    // Add disabled reason to app
    return!
        app
        |> App.addDisabledReason props.DisabledReason
        |> env.SetApp
        |> Async.AwaitTask
        |> AsyncResult.setError AddFailed
        |> AsyncResult.map _.DisabledReasons
}
