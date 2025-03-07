namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Logging
open System
open Thoth.Json.Net

type MessagingHandler (env: IEnv) =
    let [<Literal>] INVALID_SUBJECT_GUID_ERROR_MESSAGE = "Invalid guid provided as message subject"

    [<Function "NodeHeartbeat">]
    member _.NodeHeartbeat (
        [<ServiceBusTrigger "node-heartbeats">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof MessagingHandler)
        use scope = logger.BeginScope(message.Subject)

        // Ensure the message subject is a guid
        match Guid.TryParse message.Subject with
        | false, _ -> logger.LogError INVALID_SUBJECT_GUID_ERROR_MESSAGE
        | true, nodeId ->

        // Decode the message body
        match message.Body.ToString() |> Decode.fromString NodeHeartbeatMessage.decoder with
        | Error error -> logger.LogError("Failed to decode heartbeat: {Error}", error)
        | Ok data ->

        // Handle valid request
        logger.LogDebug "Received heartbeat ack"

        do! HeartbeatNodeCommand.run env {
            NodeId = nodeId
            HeartbeatTime = data.HeartbeatTime
        }

        scope.Dispose()
    }

    [<Function "NodeRelease">]
    member _.NodeRelease (
        [<ServiceBusTrigger "node-releases">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof MessagingHandler)
        use scope = logger.BeginScope(message.Subject)

        // Ensure the message subject is a guid
        match Guid.TryParse message.Subject with
        | false, _ -> logger.LogError INVALID_SUBJECT_GUID_ERROR_MESSAGE
        | true, nodeId ->

        // Handle valid request
        do! ReleaseNodeCommand.run env {
            NodeId = nodeId
        }

        scope.Dispose()
    }

    [<Function "NodeRedistribute">]
    member _.NodeRedistribute (
        [<ServiceBusTrigger "node-redistributes">] message: ServiceBusMessage,
        ctx: FunctionContext
    ) = task {
        let logger = ctx.GetLogger(nameof MessagingHandler)
        use scope = logger.BeginScope(message.Subject)

        // Ensure the message subject is a guid
        match Guid.TryParse message.Subject with
        | false, _ -> logger.LogError INVALID_SUBJECT_GUID_ERROR_MESSAGE
        | true, nodeId ->
        
        // Handle valid request
        do! RedistributeNodeCommand.run env {
            NodeId = nodeId
        }

        scope.Dispose()
    }
