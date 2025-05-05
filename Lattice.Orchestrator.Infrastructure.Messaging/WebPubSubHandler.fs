namespace Lattice.Orchestrator.Infrastructure.Messaging

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net
open Thoth.Json.Net

module WebPubSubHandler =
    let [<Literal>] HUB_NAME = "lattice-hub"

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
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.System, "connect")>] req: ConnectEventRequest,
        ctx: FunctionContext
    ) = task {
        return ()
    }

    [<Function "OnConnected">]
    member _.OnConnected (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.System, "connected")>] req: ConnectedEventRequest,
        ctx: FunctionContext
    ) = task {
        return ()
    }

    [<Function "OnDisconnect">]
    member _.OnDisconnect (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.System, "disconnect")>] req: DisconnectedEventRequest,
        ctx: FunctionContext
    ) = task {
        // TODO: Handle
        return ()
    }

    [<Function "OnShardIrrecoverableClosure">]
    member _.OnShardIrrecoverableClosure (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.User, "shard-irrecoverable-closures")>] req: UserEventRequest,
        ctx: FunctionContext
    ) = task {
        match req.Data.ToString() |> Decode.fromString ShardReceiveIrrecoverableClosureMessage.decoder with
        | Error _ ->
            return ()

        | Ok message ->
            // TODO: Handle
            return ()
    }

    [<Function "OnNodeHeartbeatPubSub">]
    member _.OnNodeHeartbeat (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.User, "node-heartbeats")>] req: UserEventRequest,
        ctx: FunctionContext
    ) = task {
        match req.Data.ToString() |> Decode.fromString NodeReceiveHeartbeatMessage.decoder with
        | Error _ ->
            return ()

        | Ok message ->
            // TODO: Handle
            return ()
    }

    [<Function "OnNodeScheduleShutdownPubSub">]
    member _.OnNodeScheduleShutdown (
        [<WebPubSubTrigger(WebPubSubHandler.HUB_NAME, WebPubSubEventType.User, "node-schedule-shutdowns")>] req: UserEventRequest,
        ctx: FunctionContext
    ) = task {
        match req.Data.ToString() |> Decode.fromString NodeReceiveScheduleShutdownMessage.decoder with
        | Error _ ->
            return ()

        | Ok message ->
            // TODO: Handle
            return ()
    }

// TODO: Rename events
