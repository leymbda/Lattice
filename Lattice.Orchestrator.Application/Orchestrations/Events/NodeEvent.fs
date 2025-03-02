namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System
open System.Threading.Tasks

type NodeEvents = {
    StartInstance: Task<ShardId * DateTime>
    ShutdownInstance: Task<ShardId * DateTime>
    SendInstanceEvent: Task<ShardId>
    TransferAllInstances: Task
    HeartbeatAck: Task<DateTime>
    HeartbeatTimeout: Task
}

type NodeEvent =
    | START_INSTANCE of shardId: ShardId * startAt: DateTime
    | SHUTDOWN_INSTANCE of shardId: ShardId * shutdownAt: DateTime
    | SEND_INSTANCE_EVENT of shardId: ShardId
    | TRANSFER_ALL_INSTANCES
    | HEARTBEAT_ACK of sentAt: DateTime
    | HEARTBEAT_TIMEOUT
    | UNKNOWN_EVENT
    
module NodeEvent =
    module Events =
        let [<Literal>] START_INSTANCE = nameof START_INSTANCE
        let [<Literal>] SHUTDOWN_INSTANCE = nameof SHUTDOWN_INSTANCE
        let [<Literal>] TRANSFER_ALL_INSTANCES = nameof TRANSFER_ALL_INSTANCES
        let [<Literal>] SEND_INSTANCE_EVENT = nameof SEND_INSTANCE_EVENT
        let [<Literal>] HEARTBEAT_ACK = nameof HEARTBEAT_ACK
        let [<Literal>] HEARTBEAT_TIMEOUT = nameof HEARTBEAT_TIMEOUT

    let [<Literal>] orchestratorName = "NodeOrchestrator"

    let orchestratorId (nodeId: Guid) =
        $"{orchestratorName}:{nodeId}"
        
    let awaitAny (events: NodeEvents) = task {
        let list: Task array = [|
            events.StartInstance
            events.ShutdownInstance
            events.SendInstanceEvent
            events.TransferAllInstances
            events.HeartbeatAck
            events.HeartbeatTimeout
        |]

        match! Task.WhenAny list with
        | event when event = events.StartInstance -> return START_INSTANCE events.StartInstance.Result
        | event when event = events.ShutdownInstance -> return SHUTDOWN_INSTANCE events.ShutdownInstance.Result
        | event when event = events.SendInstanceEvent -> return SEND_INSTANCE_EVENT events.SendInstanceEvent.Result
        | event when event = events.TransferAllInstances -> return TRANSFER_ALL_INSTANCES
        | event when event = events.HeartbeatAck -> return HEARTBEAT_ACK events.HeartbeatAck.Result
        | event when event = events.HeartbeatTimeout -> return HEARTBEAT_TIMEOUT
        | _ -> return UNKNOWN_EVENT
    }
