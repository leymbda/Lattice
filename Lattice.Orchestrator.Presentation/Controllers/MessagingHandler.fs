namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.EventGrid
open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open System

type MessagingHandler (env: IEnv) =
    [<Function "EventGridEvents">]
    member _.EventGridEvents ([<EventGridTrigger>] event: EventGridEvent) = task {
        match event.Topic with
        | Events.NODE_HEARTBEAT ->
            do! HeartbeatNodeCommand.run env {
                NodeId = Guid event.Subject
                HeartbeatTime = event.EventTime.DateTime
            }

        | _ -> ()
    }

    [<Function "NodeRelease">]
    member _.NodeRelease ([<ServiceBusTrigger "node-releases">] message: ServiceBusMessage) = task {
        do! ReleaseNodeCommand.run env {
            NodeId = Guid message.Subject
        }
    }

    [<Function "NodeRedistribute">]
    member _.NodeRedistribute ([<ServiceBusTrigger "node-redistributes">] message: ServiceBusMessage) = task {
        do! RedistributeNodeCommand.run env {
            NodeId = Guid message.Subject
        }
    }
