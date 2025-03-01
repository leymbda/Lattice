namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask.Entities
open System

type ShardEntity () =
    inherit TaskEntity<Shard>()

    [<Function(nameof ShardEntity)>]
    static member Run ([<EntityTrigger>] dispatcher: TaskEntityDispatcher) =
        dispatcher.DispatchAsync<ShardEntity>()

    member this.Create () =
        match ShardId.fromString this.Context.Id.Key with
        | None -> None
        | Some shardId ->
            this.State <- Shard.create shardId
            Some this.State

    member this.Start (startAt: DateTime) =
        // TODO: Enqueue worker node to start processing this shard as of the given time

        this.State

    member this.ScheduleShutdown shutdownAt =
        // TODO: Notify shard to stop sending events at given time

        this.State <- Shard.scheduleShutdown shutdownAt this.State
        this.State

module ShardEntity =
    let [<Literal>] SHUTDOWN_GRACE_SECONDS = 30

    let entityId shardId =
        EntityInstanceId(nameof ShardEntity, ShardId.toString shardId)

    module Operations =
        let [<Literal>] CREATE = nameof Unchecked.defaultof<ShardEntity>.Create
        let [<Literal>] START = nameof Unchecked.defaultof<ShardEntity>.Start
        let [<Literal>] SCHEDULE_SHUTDOWN = nameof Unchecked.defaultof<ShardEntity>.ScheduleShutdown
        let [<Literal>] DELETE = "Delete"
        
// TODO: Consider making different types of entities for different shard states (like existing) (?)
// TODO: Does this need to be an entity? Can it get away with being an orchestrator instead? (generally seem nicer to work with)
