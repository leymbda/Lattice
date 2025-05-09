module Lattice.WorkerNode.Node

open Elmish
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain
open System

type Model = {
    Id: Guid
    Shards: Map<ShardId, Shard.Model>
}

[<RequireQualifiedAccess>]
type Msg =
    | Shard of ShardId * Shard.Msg

    | Connect
    | OnConnectSuccess of unit
    | OnConnectError of exn

    // TODO: Shard instance schedule start/close
    // TODO: Shard irrecoverable closure bubble up (how? subscription maybe?)
    // TODO: Node schedule shutdown
    // TODO: Node heartbeat
    // TODO: Node disconnect

    | SendGatewayEvent of ShardId * GatewaySendEvent

/// Initiate connection to the orchestrator gateway
let private connect model () = async {
    return () // TODO: Implement
}

/// Handle elevated child cmd for shards
let private shard model id msg =
    match model.Shards |> Map.tryFind id with
    | None ->
        model, Cmd.none

    | Some shard ->
        let res, cmd = Shard.update msg shard

        { model with Shards = model.Shards |> Map.add id res },
        Cmd.map (fun msg -> Msg.Shard (id, msg)) cmd

/// Trigger the given shard to send the event
let private sendGatewayEvent model id event =
    match model.Shards |> Map.tryFind id with
    | None ->
        model, Cmd.none

    | Some shard ->
        model, Cmd.ofMsg (Msg.Shard (id, Shard.Msg.SendGatewayEvent event))

let init id =
    {
        Id = id
        Shards = Map.empty
    },
    Cmd.ofMsg Msg.Connect

let update msg (model: Model) =
    match msg with
    | Msg.Connect ->
        model, Cmd.OfAsync.either (connect model) () Msg.OnConnectSuccess Msg.OnConnectError

    | Msg.OnConnectSuccess _ ->
        model, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        model, Cmd.none

    | Msg.Shard (id, msg) ->
        shard model id msg

    | Msg.SendGatewayEvent (id, event) ->
        sendGatewayEvent model id event

let subscribe model =
    let shards =
        model.Shards
        |> Map.toSeq
        |> Seq.map (fun (id, model) -> "shard:" + ShardId.toString id, Shard.subscribe model)
        |> Seq.map (fun (id, sub) -> Sub.map id Msg.Shard sub)
        |> Sub.batch

    Sub.batch [
        shards
    ]
