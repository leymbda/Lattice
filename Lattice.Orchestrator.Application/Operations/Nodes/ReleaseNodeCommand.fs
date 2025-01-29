namespace Lattice.Orchestrator.Application

open System

type ReleaseNodeCommandProps = {
    NodeId: Guid
}

module ReleaseNodeCommand =
    let run (env: #IEvents) (props: ReleaseNodeCommandProps) = task {
        // TODO: Handle db logic

        do! env.NodeRelease props.NodeId
    }
