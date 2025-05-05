namespace Lattice.Orchestrator.Infrastructure.Messaging

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net
open Thoth.Json.Net

module WebPubSubHandler =
    let [<Literal>] HUB_NAME = "latticehub"

type WebPubSubHandler (env: IEnv) =
    [<Function "Negotiate">]
    member _.Negotiate (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "negotiate")>] req: HttpRequestData,
        [<WebPubSubConnectionInput>] connectionInfo: WebPubSubConnection
    ) = task {
        // TODO: Handle worker authentication

        let res = req.CreateResponse HttpStatusCode.OK
        do! res.WriteAsJsonAsync connectionInfo
        return res
    }

    [<Function "OnConnect">]
    member _.OnConnect (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.System, "connect")>] req: ConnectEventRequest
    ) = task {
        return ()
    }

    [<Function "OnConnected">]
    member _.OnConnected (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.System, "connected")>] req: ConnectedEventRequest
    ) = task {
        return ()
    }

    [<Function "OnDisconnected">]
    member _.OnDisconnected (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.System, "disconnected")>] req: DisconnectedEventRequest
    ) = task {
        // TODO: Handle
        return ()
    }

    [<Function "OnShardIrrecoverable">]
    member _.OnShardIrrecoverable (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.User, "shardIrrecoverable")>] req: UserEventRequest
    ) = task {
        return ()
        //match req.Data.ToString() |> Decode.fromString ShardReceiveIrrecoverableClosureMessage.decoder with
        //| Error _ ->
        //    return ()

        //| Ok message ->
        //    // TODO: Handle
        //    return ()
    }

    [<Function "OnHeartbeat">]
    member _.OnHeartbeat (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.User, "heartbeat")>] req: UserEventRequest
    ) = task {
        return ()
        //match req.Data.ToString() |> Decode.fromString NodeReceiveHeartbeatMessage.decoder with
        //| Error _ ->
        //    return ()

        //| Ok message ->
        //    // TODO: Handle
        //    return ()
    }

    [<Function "OnShutdownScheduled">]
    member _.OnShutdownScheduled (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.User, "shutdownScheduled")>] req: UserEventRequest
    ) = task {
        return ()
        //match req.Data.ToString() |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder with
        //| Error _ ->
        //    return ()

        //| Ok message ->
        //    // TODO: Handle
        //    return ()
    }
