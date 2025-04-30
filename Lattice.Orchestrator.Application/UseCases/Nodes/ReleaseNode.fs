module Lattice.Orchestrator.Application.ReleaseNode

open System

type Props = {
    NodeId: Guid
}

let run (env: #IEvents) props = task {
    // TODO: Handle db logic

    do! env.NodeRelease props.NodeId
}
