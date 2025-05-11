namespace Lattice.Orchestrator.Infrastructure.Pool

open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System
open System.Net
open Thoth.Json.Net

module PoolHandler =
    let [<Literal>] HUB_NAME = "latticehub"

type PoolHandler(env: IEnv) =
    [<Function "OnShardIrrecoverable">]
    member _.OnShardIrrecoverable (
        [<WebPubSubTrigger(PoolHandler.HUB_NAME, WebPubSubEventType.User, "shardIrrecoverable")>] request: UserEventRequest
    ) =
        asyncResult {
            let! message =
                request.Data.ToString()
                |> Decode.fromString ShardReceiveIrrecoverableClosureMessage.decoder
                |> Result.map (fun v -> { v with NodeId = Guid request.ConnectionContext.UserId })

            return ()
        }
        |> AsyncResult.defaultWith (fun _ -> failwith $"Failed to handle OnShardIrrecoverable event")
        |> Async.StartAsTask

    [<Function "OnHeartbeat">]
    member _.OnHeartbeat (
        [<WebPubSubTrigger(PoolHandler.HUB_NAME, WebPubSubEventType.User, "heartbeat")>] request: UserEventRequest
    ) =
        asyncResult {
            let! message =
                request.Data.ToString()
                |> Decode.fromString NodeReceiveHeartbeatMessage.decoder
                |> Result.map (fun v -> { v with NodeId = Guid request.ConnectionContext.UserId })

            return ()
        }
        |> AsyncResult.defaultWith failwith
        |> Async.StartAsTask

    [<Function "OnShutdownScheduled">]
    member _.OnShutdownScheduled (
        [<WebPubSubTrigger(PoolHandler.HUB_NAME, WebPubSubEventType.User, "shutdownScheduled")>] request: UserEventRequest
    ) =
        asyncResult {
            let! message =
                request.Data.ToString()
                |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder
                |> Result.map (fun v -> { v with NodeId = Guid request.ConnectionContext.UserId })

            return ()
        }
        |> AsyncResult.defaultWith failwith
        |> Async.StartAsTask
    
    [<Function "OnConnected">]
    member _.OnConnected (
        [<WebPubSubTrigger(PoolHandler.HUB_NAME, WebPubSubEventType.System, "connected")>] request: ConnectedEventRequest
    ) =
        asyncResult {
            let message = {
                NodeId = Guid request.ConnectionContext.UserId
                ConnectedAt = DateTime.UtcNow
            }

            return ()
        }
        |> AsyncResult.defaultWith failwith
        |> Async.StartAsTask
    
    [<Function "OnDisconnected">]
    member _.OnDisconnected (
        [<WebPubSubTrigger(PoolHandler.HUB_NAME, WebPubSubEventType.System, "disconnected")>] request: DisconnectedEventRequest
    ) =
        asyncResult {
            let message = {
                NodeId = Guid request.ConnectionContext.UserId
                DisconnectedAt = DateTime.UtcNow
            }

            return ()
        }
        |> AsyncResult.defaultWith failwith
        |> Async.StartAsTask
    
    [<Function "Negotiate">]
    member _.Negotiate (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")>] request: HttpRequestData,
        [<WebPubSubConnectionInput(Hub = PoolHandler.HUB_NAME, UserId = "{query.userId}")>] connectionInfo: WebPubSubConnection
    ) = task {
        // TODO: Handle worker authentication
        // TODO: Should this trigger be in this project or orchestrator?

        let res = request.CreateResponse HttpStatusCode.OK
        do! res.WriteAsJsonAsync connectionInfo
        return res
    }

    // TODO: Pubsub events should be defined in the contracts project to be built and sent from the worker node
