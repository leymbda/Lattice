namespace Lattice.Orchestrator.Domain

open System

type NodeHealth = {
    Id: Guid
    LastHeartbeatAck: DateTime
    ScheduledCutoff: DateTime option
    TransferReady: bool
}

module NodeHealth =
    let [<Literal>] NODE_HEARTBEAT_FREQUENCY_SECS = 30
    let [<Literal>] LIFETIME_SECONDS = NODE_HEARTBEAT_FREQUENCY_SECS * 2
    let [<Literal>] REDISTRIBUTION_CUTOFF_SECONDS = 60

    let create id currentTime = {
        Id = id
        LastHeartbeatAck = currentTime
        ScheduledCutoff = None
        TransferReady = false
    }

    let isAlive currentTime node =
        (currentTime - node.LastHeartbeatAck).TotalSeconds < float LIFETIME_SECONDS

    let heartbeat currentTime node =
        { node with LastHeartbeatAck = currentTime }

    let initiateRedistribution (currentTime: DateTime) node =
        { node with ScheduledCutoff = Some (currentTime.AddSeconds REDISTRIBUTION_CUTOFF_SECONDS) }
