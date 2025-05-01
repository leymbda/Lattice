namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Logging
open System
open Thoth.Json.Net

type ServiceBusHandler (env: IEnv) =
    let [<Literal>] INVALID_SUBJECT_SHARDID_ERROR_MESSAGE = "Invalid shard id provided as message subject"
    let [<Literal>] INVALID_SUBJECT_GUID_ERROR_MESSAGE = "Invalid guid provided as message subject"
    
    [<Function "ShardIrrecoverableClosure">]
    member _.ShardIrrecoverableClosure (
        [<ServiceBusTrigger "shard-irrecoverable-closures">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof ServiceBusHandler)
        use scope = logger.BeginScope(message.Subject)

        // Ensure the message subject is a shard id
        match ShardId.fromString message.Subject with
        | None -> logger.LogError INVALID_SUBJECT_SHARDID_ERROR_MESSAGE
        | Some shardId ->

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

        // Ensure the message subject is a guid
        match Guid.TryParse message.Subject with
        | false, _ -> logger.LogError INVALID_SUBJECT_GUID_ERROR_MESSAGE
        | true, nodeId ->

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

        // Ensure the message subject is a guid
        match Guid.TryParse message.Subject with
        | false, _ -> logger.LogError INVALID_SUBJECT_GUID_ERROR_MESSAGE
        | true, nodeId ->

        // Decode the message body
        match message.Body.ToString() |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder with
        | Error error -> logger.LogError("Failed to decode schedule shutdown: {Error}", error)
        | Ok data ->

        // Handle valid request
        logger.LogDebug "Received node schedule shutdown message"

        // TODO: Trigger handler for scheduling node shutdown

        scope.Dispose()
    }
