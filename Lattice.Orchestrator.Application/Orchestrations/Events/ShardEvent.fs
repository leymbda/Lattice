namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System
open System.Threading.Tasks

type ShardEvents = {
    CreateOrTransfer: Task<DateTime>
    Shutdown: Task<DateTime>
}

type ShardEvent =
    | CREATE_OR_TRANSFER of transferAt: DateTime
    | SHUTDOWN of shutdownAt: DateTime
    | UNKNOWN_EVENT

module ShardEvent =
    module Events =
        let [<Literal>] CREATE_OR_TRANSFER = nameof CREATE_OR_TRANSFER
        let [<Literal>] SHUTDOWN = nameof SHUTDOWN

    let [<Literal>] orchestratorName = "ShardOrchestrator"

    let orchestratorId (shardId: ShardId) =
        $"{orchestratorName}:{ShardId.toString shardId}"

    let awaitAny (events: ShardEvents) = task {
        let list: Task array = [|
            events.CreateOrTransfer
            events.Shutdown
        |]

        match! Task.WhenAny list with
        | event when event = events.CreateOrTransfer -> return CREATE_OR_TRANSFER events.CreateOrTransfer.Result
        | event when event = events.Shutdown -> return SHUTDOWN events.Shutdown.Result
        | _ -> return UNKNOWN_EVENT
    }
