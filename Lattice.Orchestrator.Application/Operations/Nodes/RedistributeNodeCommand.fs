namespace Lattice.Orchestrator.Application

open System

type RedistributeNodeCommandProps = {
    NodeId: Guid
}

module RedistributeNodeCommand =
    let run (env: #INodeEntityClient) (props: RedistributeNodeCommandProps) = task {
        // TODO: Handle db logic

        do! env.Redistribute props.NodeId
    }
