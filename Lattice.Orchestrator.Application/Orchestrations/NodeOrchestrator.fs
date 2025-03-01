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
    | CreateShard of ShardId
    | StartShard of ShardId
    | RemoveShard of ShardId * DateTime
    | Teardown

type NodeEvents = {
    Timeout: Task
    Heartbeat: Task<DateTime>
    CreateShard: Task<ShardId>
    StartShard: Task<ShardId>
    RemoveShard: Task<ShardId * DateTime>
    Teardown: Task
}

module NodeEvents =
    /// Waits for one of the given tasks to complete, and returns the event associated with it.
    let awaitEvent (events: NodeEvents) = task {
            let! winner = Task.WhenAny [|
                events.Timeout
                events.Heartbeat
                events.CreateShard
                events.StartShard
                events.RemoveShard
                events.Teardown
            |]

            return
                match winner with
                | winner when winner = events.Heartbeat -> NodeEvent.Heartbeat events.Heartbeat.Result
                | winner when winner = events.CreateShard -> NodeEvent.CreateShard events.CreateShard.Result
                | winner when winner = events.StartShard -> NodeEvent.StartShard events.StartShard.Result
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
        let ct = fctx.CancellationToken
        
        let events = {
            Timeout = ctx.CreateTimer(node.LastHeartbeatAck.AddSeconds Node.LIFETIME_SECONDS, ct)
            Heartbeat = ctx.WaitForExternalEvent<DateTime>(nameof NodeEvent.Heartbeat, ct)
            CreateShard = ctx.WaitForExternalEvent<ShardId>(nameof NodeEvent.CreateShard, ct)
            StartShard = ctx.WaitForExternalEvent<ShardId>(nameof NodeEvent.StartShard, ct)
            RemoveShard = ctx.WaitForExternalEvent<ShardId * DateTime>(nameof NodeEvent.RemoveShard, ct)
            Teardown = ctx.WaitForExternalEvent(nameof NodeEvent.Teardown, ct)
        }

        // TODO: Refactor using suborchestrations and other patterns to prevent blocking (e.g. RemoveShard's timer)

        match! NodeEvents.awaitEvent events with
        | NodeEvent.Timeout ->
            // TODO: Force release and delete

            return ()

        | NodeEvent.Heartbeat heartbeatTime ->
            let newNode = Node.heartbeat heartbeatTime node
            return ctx.ContinueAsNew newNode
            
        | NodeEvent.CreateShard shardId ->
            let! createdId = ctx.Entities.CallEntityAsync<ShardId option>(
                ShardEntity.entityId shardId,
                ShardEntity.Operations.CREATE)

            let newNode =
                match createdId with
                | None -> node
                | Some createdId -> Node.addShard createdId node

            return ctx.ContinueAsNew newNode

        | NodeEvent.StartShard shardId ->
            do! ctx.Entities.SignalEntityAsync(
                ShardEntity.entityId shardId,
                ShardEntity.Operations.START)

            return ctx.ContinueAsNew node       
            
        | NodeEvent.RemoveShard (shardId, removalTime) ->
            do! ctx.Entities.SignalEntityAsync(
                ShardEntity.entityId shardId,
                ShardEntity.Operations.SCHEDULE_SHUTDOWN,
                removalTime)

            do! ctx.CreateTimer(removalTime, ct)

            let newNode = Node.removeShard shardId node
            return ctx.ContinueAsNew newNode

        | NodeEvent.Teardown ->
            let mutable shutdownTime: DateTime option = None

            for shardId in node.Shards do
                shutdownTime <- Some (ctx.CurrentUtcDateTime.AddSeconds ShardEntity.SHUTDOWN_GRACE_SECONDS)

                do! ctx.Entities.SignalEntityAsync(
                    ShardEntity.entityId shardId,
                    ShardEntity.Operations.SCHEDULE_SHUTDOWN,
                    shutdownTime)

                // TODO: Enqueue new node to pick up the slack (where events start being processed as of `scheduledTime`)

            match shutdownTime with
            | Some time -> do! ctx.CreateTimer(time, ct)
            | None -> ()

            for shardId in node.Shards do
                do! ctx.Entities.SignalEntityAsync(
                    ShardEntity.entityId shardId,
                    ShardEntity.Operations.DELETE)

            // TODO: Nodes should just transfer ownership (property on shard model) instead of creating/deleting entities.
            //       Current implementation has a collision on the entity ID meaning this will not work. Grace period and
            //       all that should probably simply be handled by each invidual shard, where this orchestrator waits for
            //       successful transfer of all shards before ending.
    }
