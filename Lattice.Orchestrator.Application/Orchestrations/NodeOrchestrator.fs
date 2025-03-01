namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask
open System
open System.Threading.Tasks

[<RequireQualifiedAccess>]
type NodeEvent =
    | Timeout
    | Heartbeat of DateTime
    | AddShard of Guid
    | RemoveShard of Guid
    | Teardown

type NodeEvents = {
    Timeout: Task
    Heartbeat: Task<DateTime>
    AddShard: Task<Guid>
    RemoveShard: Task<Guid>
    Teardown: Task
}

module NodeEvents =
    /// Waits for one of the given tasks to complete, and returns the event associated with it.
    let awaitEvent (events: NodeEvents) = task {
            let! winner = Task.WhenAny [|
                events.Timeout
                events.Heartbeat
                events.AddShard
                events.RemoveShard
                events.Teardown
            |]

            return
                match winner with
                | winner when winner = events.Heartbeat -> NodeEvent.Heartbeat events.Heartbeat.Result
                | winner when winner = events.AddShard -> NodeEvent.AddShard events.AddShard.Result
                | winner when winner = events.RemoveShard -> NodeEvent.RemoveShard events.RemoveShard.Result
                | winner when winner = events.Teardown -> NodeEvent.Teardown
                | _ -> NodeEvent.Timeout
        }

type NodeOrchestrator () =
    [<Function(nameof NodeOrchestrator)>]
    static member Run (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        fctx: FunctionContext,
        node: Node
    ) = task {
        let events = {
            Timeout = ctx.CreateTimer(TimeSpan.FromSeconds Node.LIFETIME_SECONDS, fctx.CancellationToken)
            Heartbeat = ctx.WaitForExternalEvent<DateTime>(nameof NodeEvent.Heartbeat)
            AddShard = ctx.WaitForExternalEvent<Guid>(nameof NodeEvent.AddShard)
            RemoveShard = ctx.WaitForExternalEvent<Guid>(nameof NodeEvent.RemoveShard)
            Teardown = ctx.WaitForExternalEvent(nameof NodeEvent.Teardown)
        }

        match Node.isAlive ctx.CurrentUtcDateTime node with
        | false ->
            return ()
            
            // Check to ensure this appropriately handles race conditions. May want a new event that triggers once
            // closure is complete to end the orchestrator without preserving unprocessed events.

        | true ->
            match! NodeEvents.awaitEvent events with
            | NodeEvent.Timeout ->
                // TODO: Force release and delete
                return ()

            | NodeEvent.Heartbeat heartbeatTime ->
                let newNode = Node.heartbeat heartbeatTime node
                return ctx.ContinueAsNew(newNode)
            
            | NodeEvent.AddShard shardId ->
                // TODO: Handle shard addition

                let newNode = Node.addShard shardId node
                return ctx.ContinueAsNew(newNode)
            
            | NodeEvent.RemoveShard shardId ->
                // TODO: Handle shard removal

                let newNode = Node.removeShard shardId node
                return ctx.ContinueAsNew(newNode)

            | NodeEvent.Teardown ->
                // TODO: Soft release and delete
                return ()
    }
