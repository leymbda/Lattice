module Lattice.WorkerNode.Node

open Azure.Messaging.WebPubSub.Clients
open Elmish
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain
open System
open System.Threading.Tasks

type Model = {
    Id: Guid
    Uri: Uri
    Client: WebPubSubClient option
    Shards: Map<ShardId, Shard.Model>
    Disconnect: DateTime option option
}

// TODO: Should this be a DU to handle different states a node can be in?

[<RequireQualifiedAccess>]
type Msg =
    | Shard of ShardId * Shard.Msg

    | Connect
    | OnConnectSuccess of WebPubSubClient
    | OnConnectError of exn

    | Disconnect of DateTime option
    | OnDisconnect

    // TODO: Shard instance schedule start/close
    // TODO: Shard irrecoverable closure bubble up (how? subscription maybe?)
    // TODO: Node heartbeat

    | SendGatewayEvent of ShardId * GatewaySendEvent

/// Initiate connection to the orchestrator gateway
let private connect model () = async {
    let client = new WebPubSubClient(model.Uri)
    do! client.StartAsync() |> Async.AwaitTask
    return client
}

/// Handle elevated child cmd for shards
let private shard model id msg =
    match model.Shards |> Map.tryFind id with
    | None ->
        model, Cmd.none

    | Some shard ->
        let res, cmd = Shard.update msg shard

        let shards =
            match msg with
            | Shard.Msg.OnDisconnect Shard.DisconnectType.Requested ->
                model.Shards |> Map.remove id

            | Shard.Msg.OnDisconnect Shard.DisconnectType.Irrecoverable ->
                model.Shards |> Map.remove id

                // TODO: Notify orchestrator of irrecoverable closure (new cmd msg)

            | _ ->
                model.Shards |> Map.add id res

        { model with Shards = shards },
        Cmd.map (fun msg -> Msg.Shard (id, msg)) cmd

/// Trigger the given shard to send the event
let private sendGatewayEvent model id event =
    model, Cmd.ofMsg (Msg.Shard (id, Shard.Msg.SendGatewayEvent event))

let init (id, uri) =
    {
        Id = id
        Uri = uri
        Client = None
        Shards = Map.empty
        Disconnect = None
    },
    Cmd.ofMsg Msg.Connect

let update msg (model: Model) =
    match msg with
    | Msg.Shard (id, msg) ->
        shard model id msg

    | Msg.Connect ->
        model, Cmd.OfAsync.either (connect model) () Msg.OnConnectSuccess Msg.OnConnectError

    | Msg.OnConnectSuccess client ->
        printfn "Successfully connected node %A" model.Id
        { model with Client = Some client }, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        model, Cmd.ofMsg (Msg.Disconnect None)

    | Msg.Disconnect shutdownAt ->
        { model with Disconnect = Some shutdownAt },
        model.Shards
        |> Map.toSeq
        |> Seq.map (fun (id, _) ->
            Msg.Shard (id, Shard.Msg.Disconnect (Shard.DisconnectType.Requested, shutdownAt)) |> Cmd.ofMsg
        )
        |> Cmd.batch

    | Msg.OnDisconnect ->
        printfn "Disconnected node %A" model.Id
        { model with Client = None }, Cmd.none

    | Msg.SendGatewayEvent (id, event) ->
        sendGatewayEvent model id event

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
        match model.Disconnect with
        | Some shutdownAt ->
            let timespan =
                shutdownAt
                |> Option.map (_.Subtract(DateTime.UtcNow))
                |> Option.defaultValue (TimeSpan.FromSeconds 0)

            let onShutdown dispatch () =
                model.Client
                |> Option.map (_.StopAsync())
                |> Option.defaultValue Task.CompletedTask
                |> Async.AwaitTask
                |> Async.RunSynchronously
                
                // TODO: Make this an async function itself

                dispatch Msg.OnDisconnect

            [["shutdown"], fun dispatch -> Sub.delay timespan (onShutdown dispatch)]

        | _ -> []

    List.empty
    |> List.append shards
    |> List.append shutdown

let terminate msg =
    match msg with
    | Msg.OnDisconnect -> true
    | _ -> false
