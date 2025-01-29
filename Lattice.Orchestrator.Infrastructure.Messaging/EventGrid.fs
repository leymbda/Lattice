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
