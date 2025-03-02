namespace Lattice.Orchestrator.Domain

open System

type NodeState =
    | Active
    | Expired

type Node = {
    Id: Guid
    LastHeartbeatAck: DateTime
}

module Node =
    let [<Literal>] NODE_HEARTBEAT_FREQUENCY_SECS = 30
    let [<Literal>] LIFETIME_SECONDS = NODE_HEARTBEAT_FREQUENCY_SECS * 2

    let create id currentTime = {
        Id = id
        LastHeartbeatAck = currentTime
    }

    let heartbeat currentTime node =
        { node with LastHeartbeatAck = currentTime }
        
    let getState (currentTime: DateTime) node =
        match node with
        | { LastHeartbeatAck = lastHeartbeatAck } when lastHeartbeatAck.AddSeconds LIFETIME_SECONDS < currentTime -> Expired
        | _ -> Active
