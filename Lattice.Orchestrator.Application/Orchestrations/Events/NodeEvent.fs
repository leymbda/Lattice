namespace Lattice.Orchestrator.Application

open System
open System.Threading.Tasks

type NodeEvents = {
    TransferAllInstances: Task
    HeartbeatAck: Task<DateTime>
    HeartbeatTimeout: Task
}

type NodeEvent =
    | TRANSFER_ALL_INSTANCES
    | HEARTBEAT_ACK of sentAt: DateTime
    | HEARTBEAT_TIMEOUT
    | UNKNOWN_EVENT
    
module NodeEvent =
    module Events =
        let [<Literal>] TRANSFER_ALL_INSTANCES = nameof TRANSFER_ALL_INSTANCES
        let [<Literal>] HEARTBEAT_ACK = nameof HEARTBEAT_ACK
        let [<Literal>] HEARTBEAT_TIMEOUT = nameof HEARTBEAT_TIMEOUT

    let [<Literal>] orchestratorName = "NodeOrchestrator"

    let orchestratorId (nodeId: Guid) =
        $"{orchestratorName}:{nodeId}"
        
    let awaitAny (events: NodeEvents) = task {
        let list: Task array = [|
            events.TransferAllInstances
            events.HeartbeatAck
            events.HeartbeatTimeout
        |]

        match! Task.WhenAny list with
        | event when event = events.TransferAllInstances -> return TRANSFER_ALL_INSTANCES
        | event when event = events.HeartbeatAck -> return HEARTBEAT_ACK events.HeartbeatAck.Result
        | event when event = events.HeartbeatTimeout -> return HEARTBEAT_TIMEOUT
        | _ -> return UNKNOWN_EVENT
    }
