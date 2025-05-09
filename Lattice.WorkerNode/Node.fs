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
    | OnConnectSuccess
    | OnConnectError of exn

    | Disconnect // TODO: Add optional DateTime to schedule shutdown
    | OnDisconnect

    // TODO: Shard instance schedule start/close
    // TODO: Shard irrecoverable closure bubble up (how? subscription maybe?)
    // TODO: Node heartbeat

    | SendGatewayEvent of ShardId * GatewaySendEvent

/// Initiate connection to the orchestrator gateway
let private connect model () = async {
    return () // TODO: Implement
}

let private disconnect model () = async {
    return () // TODO: Implement

    // This probably needs to be changed to instead send a batch command to all shards to disconnect with some state
    // indicating the shutdown was requested. Once all shards removed and shutdown IS requested, trigger disconnect.
    // Seems like a bit of a rewrite to what's currently implemented for disconnecting behaviour but will keep this in
    // for now as a reference.
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
    | Msg.Shard (id, msg) ->
        shard model id msg

    | Msg.Connect ->
        model, Cmd.OfAsync.either (connect model) () (fun _ -> Msg.OnConnectSuccess) Msg.OnConnectError

    | Msg.OnConnectSuccess ->
        printfn "Successfully connected node %A" model.Id
        model, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        model, Cmd.none

    | Msg.Disconnect ->
        model, Cmd.OfAsync.perform (disconnect model) () (fun _ -> Msg.OnDisconnect) // TODO: Should this handle potential error too?

        // TODO: This should probably batch trigger Disconnect msg on all shards

    | Msg.OnDisconnect ->
        printfn "Disconnected node %A" model.Id
        model, Cmd.none

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

    Sub.batch [
        shards
    ]
