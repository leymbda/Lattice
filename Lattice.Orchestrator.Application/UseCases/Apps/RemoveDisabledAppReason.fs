module Lattice.Orchestrator.Application.RemoveDisabledAppReason

open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Domain

type Props = {
    AppId: string
    DisabledReason: DisabledAppReason
}

type Failure =
    | AppNotFound
    | RemoveFailed
    
let run (env: #IPersistence) props = asyncResult {
    // Fetch app from db
    let! app =
        env.GetApp props.AppId
        |> Async.AwaitTask
        |> AsyncResult.setError AppNotFound
        
    // Remove disabled reason from app
    return!
        app
        |> App.removeDisabledReason props.DisabledReason
        |> env.SetApp
        |> Async.AwaitTask
        |> AsyncResult.setError RemoveFailed
        |> AsyncResult.map _.DisabledReasons
}
