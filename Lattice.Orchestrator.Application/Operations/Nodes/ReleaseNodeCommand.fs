namespace Lattice.Orchestrator.Application

open System

type ReleaseNodeCommandProps = {
    NodeId: Guid
}

module ReleaseNodeCommand =
    let run (env: #INodeEntityClient) (props: ReleaseNodeCommandProps) = task {
        // TODO: Handle db logic

        do! env.Release props.NodeId
    }
