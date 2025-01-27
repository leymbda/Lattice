namespace Lattice.Orchestrator.Application

open System

type HeartbeatNodeCommandProps = {
    NodeId: Guid
}

module HeartbeatNodeCommand =
    let run (env: #INodeEntityClient) (props: HeartbeatNodeCommandProps) = task {
        do! env.Heartbeat props.NodeId
    }
