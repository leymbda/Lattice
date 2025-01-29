namespace Lattice.Orchestrator.Application

open System

type RedistributeNodeCommandProps = {
    NodeId: Guid
}

module RedistributeNodeCommand =
    let run (env: #IEvents) (props: RedistributeNodeCommandProps) = task {
        // TODO: Handle db logic

        do! env.NodeRedistribute props.NodeId
    }
