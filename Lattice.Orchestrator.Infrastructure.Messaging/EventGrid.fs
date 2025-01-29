module Lattice.Orchestrator.Infrastructure.Messaging.EventGrid

open Azure.Messaging.EventGrid
open Lattice.Orchestrator.Application
open System
open System.Threading
open System.Threading.Tasks

let nodeHeartbeat (publisher: EventGridPublisherClient) (nodeId: Guid) (heartbeatTime: DateTime) = task {
    let event = new EventGridEvent(
        nodeId.ToString(),
        Events.NODE_HEARTBEAT,
        "1.0.0",
        heartbeatTime
    )

    do! publisher.SendEventAsync(event, CancellationToken.None) :> Task
}

let nodeRelease (publisher: EventGridPublisherClient) (nodeId: Guid) = task {
    let event = new EventGridEvent(
        nodeId.ToString(),
        Events.NODE_RELEASE,
        "1.0.0",
        "" // TODO: Should this include all the shard info?
    )

    do! publisher.SendEventAsync(event, CancellationToken.None) :> Task
}

let nodeRedistribute (publisher: EventGridPublisherClient) (nodeId: Guid) = task {
    let event = new EventGridEvent(
        nodeId.ToString(),
        Events.NODE_REDISTRIBUTE,
        "1.0.0",
        "" // TODO: Should this include all the shard info?
    )

    do! publisher.SendEventAsync(event, CancellationToken.None) :> Task
}

// TODO: Should release and redistribute use service bus instead? Is this feasible?
