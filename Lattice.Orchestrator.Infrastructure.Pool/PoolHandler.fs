module Lattice.Orchestrator.Infrastructure.Pool.PoolHandler

open Azure.Messaging.ServiceBus
open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System
open System.Net
open Thoth.Json.Net

let [<Literal>] HUB_NAME = "latticehub"

[<Function "ReceivePoolInboundMessage">]
[<WebPubSubOutput(Hub = HUB_NAME)>]
let receivePoolInboundMessage (
    [<ServiceBusTrigger(PoolInboundMessage.queueName)>] message: ServiceBusReceivedMessage
) =
    match Decode.fromString PoolInboundMessage.decoder (message.Body.ToString()) with
    | Ok (PoolInboundMessage.ShardInstanceScheduleStart m) ->
        WebPubSubAction.sendToUser
            (m.NodeId.ToString())
            (m |> ShardInstanceSendScheduleStartMessage.encoder |> Encode.toString 0)

    | Ok (PoolInboundMessage.ShardInstanceScheduleClose m) ->
        WebPubSubAction.sendToUser
            (m.NodeId.ToString())
            (m |> ShardInstanceSendScheduleCloseMessage.encoder |> Encode.toString 0)

    | Ok (PoolInboundMessage.ShardInstanceGatewayEvent m) ->
        WebPubSubAction.sendToUser
            (m.NodeId.ToString())
            (m |> ShardInstanceSendGatewayEventMessage.encoder |> Encode.toString 0)

    | _ -> failwith $"Invalid {nameof PoolInboundMessage} received"

[<Function "ReceivePoolOutboundMessage">]
let receivePoolOutboundMessage (
    [<ServiceBusTrigger(PoolOutboundMessage.queueName)>] message: ServiceBusReceivedMessage
) =
    match Decode.fromString PoolOutboundMessage.decoder (message.Body.ToString()) with
    | Ok (PoolOutboundMessage.ShardIrrecoverableClosure m) ->
        raise (NotImplementedException())
        ()

    | Ok (PoolOutboundMessage.NodeHeartbeat m) ->
        raise (NotImplementedException())
        ()

    | Ok (PoolOutboundMessage.NodeScheduleShutdown m) ->
        raise (NotImplementedException())
        ()

    | _ -> failwith $"Invalid {nameof PoolOutboundMessage} received"

    // TODO: Call application use cases for these events above
    // TODO: Is this service bus necessary? Extra layer of abstraction on pubsub events below

[<Function "OnShardIrrecoverable">]
[<ServiceBusOutput(PoolOutboundMessage.queueName)>]
let onShardIrrecoverable (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.User, "shardIrrecoverable")>] req: UserEventRequest
) =
    req.Data.ToString()
    |> Decode.fromString ShardReceiveIrrecoverableClosureMessage.decoder
    |> Result.map (fun v -> { v with NodeId = Guid req.ConnectionContext.UserId })
    |> Result.defaultWith (failwith $"Invalid {nameof ShardReceiveIrrecoverableClosureMessage} received")
    |> ShardReceiveIrrecoverableClosureMessage.encoder
    |> Encode.toString 0

[<Function "OnHeartbeat">]
[<ServiceBusOutput(PoolOutboundMessage.queueName)>]
let onHeartbeat (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.User, "heartbeat")>] req: UserEventRequest
) =
    req.Data.ToString()
    |> Decode.fromString NodeReceiveHeartbeatMessage.decoder
    |> Result.map (fun v -> { v with NodeId = Guid req.ConnectionContext.UserId })
    |> Result.defaultWith (failwith $"Invalid {nameof NodeReceiveHeartbeatMessage} received")
    |> NodeReceiveHeartbeatMessage.encoder
    |> Encode.toString 0

[<Function "OnShutdownScheduled">]
[<ServiceBusOutput(PoolOutboundMessage.queueName)>]
let onShutdownScheduled (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.User, "shutdownScheduled")>] req: UserEventRequest
) =
    req.Data.ToString()
    |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder
    |> Result.map (fun v -> { v with NodeId = Guid req.ConnectionContext.UserId })
    |> Result.defaultWith (failwith $"Invalid {nameof NodeReceiveScheduleShutdownMessage} received")
    |> NodeReceiveScheduleShutdownMessage.encoder
    |> Encode.toString 0
    
[<Function "OnConnected">]
[<ServiceBusOutput(PoolOutboundMessage.queueName)>]
let onConnected (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.System, "connected")>] req: ConnectedEventRequest
) =
    {
        NodeId = Guid req.ConnectionContext.UserId
        ConnectedAt = DateTime.UtcNow
    }
    |> NodeConnectedMessage.encoder
    |> Encode.toString 0
    
[<Function "OnDisconnected">]
[<ServiceBusOutput(PoolOutboundMessage.queueName)>]
let onDisconnected (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.System, "disconnected")>] req: DisconnectedEventRequest
) =
    {
        NodeId = Guid req.ConnectionContext.UserId
        DisconnectedAt = DateTime.UtcNow
    }
    |> NodeDisconnectedMessage.encoder
    |> Encode.toString 0
    
[<Function "Negotiate">]
let negotiate (
    [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")>] req: HttpRequestData,
    [<WebPubSubConnectionInput>] connectionInfo: WebPubSubConnection
) = task {
    // TODO: Handle worker authentication
    // TODO: Should this trigger be in this project or orchestrator?

    let res = req.CreateResponse HttpStatusCode.OK
    do! res.WriteAsJsonAsync connectionInfo
    return res
}

// TODO: Pubsub events should be defined in the contracts project to be built and sent from the worker node
