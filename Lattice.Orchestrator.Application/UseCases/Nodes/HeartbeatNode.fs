module Lattice.Orchestrator.Application.HeartbeatNode

open System

type Props = {
    NodeId: Guid
    HeartbeatTime: DateTime
}

let run (env: #IEvents) props = task {
    // TODO: Handle db logic (upsert as this also handles registration)

    do! env.NodeHeartbeat props.NodeId props.HeartbeatTime
}
