namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System
open System.Threading.Tasks

type ShardInstanceEvents = {
    Start: Task<DateTime>
    Shutdown: Task<DateTime>
}

type ShardInstanceEvent =
    | START of startAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime
    | UNKNOWN_EVENT
    
module ShardInstanceEvent =
    module Events =
        let [<Literal>] START = nameof START
        let [<Literal>] SHUTDOWN = nameof SHUTDOWN

    let [<Literal>] orchestratorName = "ShardInstanceOrchestrator"

    let orchestratorId (shardId: ShardId) (nodeId: Guid) =
        $"{orchestratorName}:{ShardId.toString shardId}:{nodeId}"
        
    let awaitAny (events: ShardInstanceEvents) = task {
        let list: Task array = [|
            events.Start
            events.Shutdown
        |]

        match! Task.WhenAny list with
        | event when event = events.Start -> return START events.Start.Result
        | event when event = events.Shutdown -> return SHUTDOWN events.Shutdown.Result
        | _ -> return UNKNOWN_EVENT
    }
