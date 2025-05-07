module Lattice.Orchestrator.Infrastructure.Pool.PoolHandler

open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System
open System.Net
open Thoth.Json.Net

let [<Literal>] HUB_NAME = "latticehub"

[<Function "OnShardIrrecoverable">]
let onShardIrrecoverable (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.User, "shardIrrecoverable")>] req: UserEventRequest
) =
    let message =
        req.Data.ToString()
        |> Decode.fromString ShardReceiveIrrecoverableClosureMessage.decoder
        |> Result.map (fun v -> { v with NodeId = Guid req.ConnectionContext.UserId })
        |> Result.defaultWith (failwith $"Invalid {nameof ShardReceiveIrrecoverableClosureMessage} received")

    raise (NotImplementedException()) // TODO: Call use case
    ()

[<Function "OnHeartbeat">]
let onHeartbeat (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.User, "heartbeat")>] req: UserEventRequest
) =
    let message =
        req.Data.ToString()
        |> Decode.fromString NodeReceiveHeartbeatMessage.decoder
        |> Result.map (fun v -> { v with NodeId = Guid req.ConnectionContext.UserId })
        |> Result.defaultWith (failwith $"Invalid {nameof NodeReceiveHeartbeatMessage} received")

    raise (NotImplementedException()) // TODO: Call use case
    ()

[<Function "OnShutdownScheduled">]
let onShutdownScheduled (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.User, "shutdownScheduled")>] req: UserEventRequest
) =
    let message =
        req.Data.ToString()
        |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder
        |> Result.map (fun v -> { v with NodeId = Guid req.ConnectionContext.UserId })
        |> Result.defaultWith (failwith $"Invalid {nameof NodeReceiveScheduleShutdownMessage} received")

    raise (NotImplementedException()) // TODO: Call use case
    ()
    
[<Function "OnConnected">]
let onConnected (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.System, "connected")>] req: ConnectedEventRequest
) =
    let message =
        {
            NodeId = Guid req.ConnectionContext.UserId
            ConnectedAt = DateTime.UtcNow
        }

    raise (NotImplementedException()) // TODO: Call use case
    ()
    
[<Function "OnDisconnected">]
let onDisconnected (
    [<WebPubSubTrigger(HUB_NAME, WebPubSubEventType.System, "disconnected")>] req: DisconnectedEventRequest
) =
    let message =
        {
            NodeId = Guid req.ConnectionContext.UserId
            DisconnectedAt = DateTime.UtcNow
        }

    raise (NotImplementedException()) // TODO: Call use case
    ()
    
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
