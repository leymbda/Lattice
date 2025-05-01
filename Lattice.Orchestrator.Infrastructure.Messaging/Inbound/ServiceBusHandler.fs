namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Logging
open Thoth.Json.Net

type ServiceBusHandler (env: IEnv) =
    [<Function "ShardIrrecoverableClosure">]
    member _.ShardIrrecoverableClosure (
        [<ServiceBusTrigger "shard-irrecoverable-closures">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof ServiceBusHandler)
        use scope = logger.BeginScope(message.Subject)
        
        // Decode the message body
        match message.Body.ToString() |> Decode.fromString ShardReceiveIrrecoverableClosureMessage.decoder with
        | Error error -> logger.LogError("Failed to decode irrecoverable shard closure: {Error}", error)
        | Ok data ->

        // Handle valid request
        logger.LogDebug "Received shard irrecoverable closure message"

        // TODO: Trigger handler for irrecoverable shard closure

        scope.Dispose()
    }

    [<Function "NodeHeartbeat">]
    member _.NodeHeartbeat (
        [<ServiceBusTrigger "node-heartbeats">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof ServiceBusHandler)
        use scope = logger.BeginScope(message.Subject)

        // Decode the message body
        match message.Body.ToString() |> Decode.fromString NodeReceiveHeartbeatMessage.decoder with
        | Error error -> logger.LogError("Failed to decode heartbeat: {Error}", error)
        | Ok data ->

        // Handle valid request
        logger.LogDebug "Received heartbeat message"

        // TODO: Trigger handler for heartbeats

        scope.Dispose()
    }

    [<Function "NodeScheduleShutdown">]
    member _.NodeScheduleShutdown (
        [<ServiceBusTrigger "node-schedule-shutdowns">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof ServiceBusHandler)
        use scope = logger.BeginScope(message.Subject)

        // Decode the message body
        match message.Body.ToString() |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder with
        | Error error -> logger.LogError("Failed to decode schedule shutdown: {Error}", error)
        | Ok data ->

        // Handle valid request
        logger.LogDebug "Received node schedule shutdown message"

        // TODO: Trigger handler for scheduling node shutdown

        scope.Dispose()
    }
