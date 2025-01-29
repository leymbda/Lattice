module Lattice.Orchestrator.Infrastructure.Messaging.ServiceBus

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open System

let nodeRelease (client: ServiceBusClient) (nodeId: Guid) = task {
    let sender = client.CreateSender("")

    let message = new ServiceBusMessage(Events.NODE_RELEASE)
    do! sender.SendMessageAsync(message)
}

let nodeRedistribute (client: ServiceBusClient) (nodeId: Guid) = task {
    let sender = client.CreateSender("")

    let message = new ServiceBusMessage(Events.NODE_REDISTRIBUTE)
    do! sender.SendMessageAsync(message)
}
