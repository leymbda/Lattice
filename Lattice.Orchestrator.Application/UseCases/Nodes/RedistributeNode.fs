module Lattice.Orchestrator.Application.RedistributeNode

open FsToolkit.ErrorHandling
open System

type Props = {
    NodeId: Guid
}

let run (env: #IEvents) props = asyncResult {
    // TODO: Handle db logic

    do! env.NodeRedistribute props.NodeId
}
