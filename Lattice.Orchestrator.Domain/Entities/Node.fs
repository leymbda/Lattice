namespace Lattice.Orchestrator.Domain

open System

type Node = {
    Id: string
    LastHeartbeatAck: DateTime
}

module Node =
    let [<Literal>] NODE_LIFETIME_SECONDS = 60

    let create id currentTime =
        {
            Id = id
            LastHeartbeatAck = currentTime
        }

    let isAlive currentTime node =
        (currentTime - node.LastHeartbeatAck).TotalSeconds < float NODE_LIFETIME_SECONDS

    let heartbeat currentTime node =
        { node with LastHeartbeatAck = currentTime }
