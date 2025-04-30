module Lattice.Orchestrator.Application.RedistributeNode

open System

type Props = {
    NodeId: Guid
}

let run (env: #IEvents) props = task {
    // TODO: Handle db logic

    do! env.NodeRedistribute props.NodeId
}
