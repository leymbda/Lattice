module Lattice.Orchestrator.Application.ReleaseNode

open FsToolkit.ErrorHandling
open System

type Props = {
    NodeId: Guid
}

let run (env: #IEvents) props = asyncResult {
    // TODO: Handle db logic

    do! env.NodeRelease props.NodeId
}
