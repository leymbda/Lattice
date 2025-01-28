namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.EventGrid
open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open System

type EventGridHandler (env: IEnv) =
    [<Function(nameof EventGridHandler)>]
    member _.Run ([<EventGridTrigger>] event: EventGridEvent) = task {
        match event.Topic with
        | Events.NODE_HEARTBEAT ->
            do! HeartbeatNodeCommand.run env {
                NodeId = Guid event.Subject
                HeartbeatTime = event.EventTime.DateTime
            }

        | Events.NODE_RELEASE ->
            do! ReleaseNodeCommand.run env {
                NodeId = Guid event.Subject
            }

        | Events.NODE_REDISTRIBUTE ->
            do! RedistributeNodeCommand.run env {
                NodeId = Guid event.Subject
            }

        | _ -> ()
    }
