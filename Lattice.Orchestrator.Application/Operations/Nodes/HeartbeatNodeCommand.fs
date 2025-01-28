namespace Lattice.Orchestrator.Application

open System

type HeartbeatNodeCommandProps = {
    NodeId: Guid
    HeartbeatTime: DateTime
}

module HeartbeatNodeCommand =
    let run (env: #INodeEntityClient) (props: HeartbeatNodeCommandProps) = task {
        // TODO: Handle db logic (upsert as this also handles registration)

        do! env.Heartbeat props.NodeId props.HeartbeatTime
    }
