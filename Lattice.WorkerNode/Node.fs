module Lattice.WorkerNode.Node

open Azure.Messaging.WebPubSub.Clients
open Elmish
open FSharp.Discord.Gateway
open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
open System
open System.Net.Http
open System.Threading.Tasks
open Thoth.Json.Net

type Model = {
    Id: Guid
    NegotiateUri: Uri
    Client: WebPubSubClient option
    Shards: Map<ShardId, Shard.Model>
    SendNextHeartbeatAt: DateTime option
    DisconnectRequestedFor: DateTime option
    ConnectedAt: DateTime option
    DisconnectedAt: DateTime option
}

[<RequireQualifiedAccess>]
type Msg =
    | Shard of ShardId * Shard.Msg

    | Connect of negotiateUri: Uri
    | OnConnectSuccess of client: WebPubSubClient * connectedAt: DateTime
    | OnConnectError of exn

    | ScheduleDisconnect of DateTime option
    | Disconnect
    | OnDisconnect

    | SendGatewayEvent of ShardId * GatewaySendEvent
    | ShardIrrecoverableClosure of ShardId
    | ShardScheduleStart of id: ShardId * startAt: DateTime
    | ShardScheduleClose of id: ShardId * shutdownAt: DateTime
    | Heartbeat

let init (id, negotiateUri) =
    {
        Id = id
        NegotiateUri = negotiateUri
        Client = None
        Shards = Map.empty
        SendNextHeartbeatAt = None
        DisconnectRequestedFor = None
        ConnectedAt = None
        DisconnectedAt = None
    },
    Cmd.ofMsg (Msg.Connect negotiateUri)

let update msg (model: Model) =
    match msg with
    | Msg.Connect negotiateUri ->
        let connect (negotiateUri: Uri) =
            asyncResult {
                use client = new HttpClient()
                let! res = client.PostAsync(negotiateUri, null) |> Async.AwaitTask
                let! content = res.Content.ReadAsStringAsync() |> Async.AwaitTask
                let! negotiate = Decode.fromString NegotiateResponse.decoder content

                let client = new WebPubSubClient(Uri negotiate.Url)
                do! client.StartAsync()

                return client, DateTime.UtcNow
            }
            |> AsyncResult.defaultWith failwith

        model, Cmd.OfAsync.either connect negotiateUri Msg.OnConnectSuccess Msg.OnConnectError

    | Msg.OnConnectSuccess (client, connectedAt) ->
        printfn "Successfully connected node %A" model.Id
        { model with
            Client = Some client
            SendNextHeartbeatAt = Some DateTime.UtcNow
            ConnectedAt = Some connectedAt },
        Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        model, Cmd.ofMsg Msg.OnDisconnect

    | Msg.ScheduleDisconnect shutdownAt ->
        let shutdownAt = Option.defaultValue DateTime.UtcNow shutdownAt

        { model with DisconnectRequestedFor = Some shutdownAt },
        model.Shards
        |> Map.toSeq
        |> Seq.map (fun (id, _) -> Msg.Shard (id, Shard.Msg.ScheduleDisconnect (Shard.DisconnectType.Requested, shutdownAt)) |> Cmd.ofMsg)
        |> Cmd.batch

    | Msg.Disconnect ->
        let disconnect () =
            asyncResult {
                let! client = model.Client |> Result.requireSome ()
                do! client.StopAsync()
            }
            |> AsyncResult.ignoreError

        model, Cmd.OfAsync.perform disconnect () (fun _ -> Msg.OnDisconnect)

    | Msg.OnDisconnect ->
        printfn "Disconnected node %A" model.Id
        { model with
            Client = None
            Shards = Map.empty
            DisconnectRequestedFor = None
            DisconnectedAt = Some DateTime.UtcNow },
        Cmd.none
        
    | Msg.Shard (id, msg) ->
        match model.Shards |> Map.tryFind id with
        | None ->
            eprintfn "Attempted shard operation on non-existed shard %s" (ShardId.toString id)
            model, Cmd.none

        | Some shard ->
            let res, cmd = Shard.update msg shard

            match msg with
            | Shard.Msg.OnDisconnect Shard.DisconnectType.Requested ->
                { model with Shards = model.Shards |> Map.remove id },
                Cmd.map (fun msg -> Msg.Shard (id, msg)) cmd

            | Shard.Msg.OnDisconnect Shard.DisconnectType.Irrecoverable ->
                { model with Shards = model.Shards |> Map.remove id },
                Cmd.batch [
                    Cmd.map (fun msg -> Msg.Shard (id, msg)) cmd
                    Cmd.ofMsg (Msg.ShardIrrecoverableClosure id)
                ]

            | _ ->
                { model with Shards = model.Shards |> Map.add id res },
                Cmd.map (fun msg -> Msg.Shard (id, msg)) cmd

    | Msg.ShardIrrecoverableClosure id ->
        // TODO: Notify orchestrator of irrecoverable closure
        model, Cmd.none
        
    | Msg.ShardScheduleStart (id, startAt) ->
        let res, cmd = Shard.init id startAt

        { model with Shards = model.Shards |> Map.add id res },
        Cmd.map (fun msg -> Msg.Shard (id, msg)) cmd
        
    | Msg.ShardScheduleClose (id, shutdownAt) ->
        model, Cmd.map (fun msg -> Msg.Shard (id, msg)) (Cmd.ofMsg (Shard.Msg.ScheduleDisconnect (Shard.DisconnectType.Requested, shutdownAt)))

    | Msg.Heartbeat ->
        // TODO: Notify orchestrator of currently connected shard IDs
        { model with SendNextHeartbeatAt = Some (DateTime.UtcNow.AddSeconds 30) }, Cmd.none // TODO: How long should this wait before next?

    | Msg.SendGatewayEvent (id, event) ->
        model, Cmd.ofMsg (Msg.Shard (id, Shard.Msg.SendGatewayEvent event))

let subscribe model =
    let shards =
        model.Shards
        |> Map.toSeq
        |> Seq.map (fun (id, model) ->
            let idPrefix = "shard:" + ShardId.toString id // TODO: Does this need to contain all this?
            let mapper = fun msg -> Msg.Shard (id, msg)
            let sub = Shard.subscribe model
            Sub.map idPrefix mapper sub
        )
        |> Sub.batch

    let shutdown =
        match model.DisconnectRequestedFor with
        | None -> []
        | Some shutdownAt ->
            [["shutdown"], fun dispatch -> Sub.delay (shutdownAt.Subtract DateTime.UtcNow) (fun _ -> dispatch Msg.Disconnect)]

    let disconnect =
        match model.Client with
        | None -> []
        | Some client ->
            let sub dispatch =
                let handler _ =
                    dispatch Msg.OnDisconnect |> Task.FromResult :> Task

                client.add_Disconnected handler
                { new IDisposable with member _.Dispose () = client.remove_Disconnected handler }

            [["disconnect"], sub]

    let heartbeat =
        match model.SendNextHeartbeatAt with
        | None -> []
        | Some sendAt ->
            [["heartbeat"; sendAt.ToString()], fun dispatch -> Sub.delay (sendAt.Subtract DateTime.UtcNow) (fun _ -> dispatch Msg.Heartbeat)]
            
    let serverEvent =
        match model.Client with
        | None -> []
        | Some client ->
            let sub dispatch =
                let handler (args: WebPubSubServerMessageEventArgs) = // TODO: Is ServerMessageReceived the correct type for these?
                    // TODO: Read payload and dispatch appropriate message
                    () |> Task.FromResult :> Task

                client.add_ServerMessageReceived handler
                { new IDisposable with member _.Dispose () = client.remove_ServerMessageReceived handler }

            [["disconnect"], sub]

    Sub.batch [
        shards
        shutdown
        disconnect
        heartbeat
        serverEvent
    ]

let terminate msg =
    match msg with
    | Msg.OnDisconnect -> true
    | _ -> false
