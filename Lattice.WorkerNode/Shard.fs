module Lattice.WorkerNode.Shard

open Elmish
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain

type Model = {
    Id: ShardId
}

type DisconnectType =
    | Requested
    | Unexpected
    | Irrecoverable

[<RequireQualifiedAccess>]
type Msg =
    | Connect
    | OnConnectSuccess
    | OnConnectError of exn

    | Disconnect of type': DisconnectType // TODO: Add optional DateTime to schedule disconnect
    | OnDisconnect of type': DisconnectType

    | SendGatewayEvent of GatewaySendEvent
    | OnSendGatewayEventError of exn

    | ReceiveGatewayEvent of GatewayReceiveEvent
    | OnReceiveGatewayEventError of exn

/// Initiate connection to the gateway
let private connect model () = async {
    return () // TODO: Implement
}

/// Gracefully disconenct from the gateway
let private disconnect model type' = async {
    return type' // TODO: Implement
}

/// Send a gateway event to the gateway
let private sendGatewayEvent model event = async {
    return () // TODO: Implement
}

/// Handle received gateway event by sending to orchestrated handler
let private receiveGatewayEvent model event = async {
    return () // TODO: Implement
}

let init id =
    { Id = id }, Cmd.ofMsg Msg.Connect

let update msg (model: Model) =
    match msg with
    | Msg.Connect ->
        model, Cmd.OfAsync.either (connect model) () (fun _ -> Msg.OnConnectSuccess) Msg.OnConnectError

    | Msg.OnConnectSuccess ->
        printfn "Successfully connected shard %s to the gateway" (ShardId.toString model.Id)
        model, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        model, Cmd.none

    | Msg.Disconnect type' ->
        model, Cmd.OfAsync.perform (disconnect model) type' Msg.OnDisconnect

    | Msg.OnDisconnect type' ->
        printfn "Disconnected shard %s from the gateway" (ShardId.toString model.Id)
        model, Cmd.none

    | Msg.SendGatewayEvent event ->
        model, Cmd.OfAsync.attempt (sendGatewayEvent model) event Msg.OnSendGatewayEventError

    | Msg.OnSendGatewayEventError exn ->
        eprintfn "%A" exn
        model, Cmd.none

    | Msg.ReceiveGatewayEvent event ->
        model, Cmd.OfAsync.attempt (receiveGatewayEvent model) event Msg.OnReceiveGatewayEventError
        
    | Msg.OnReceiveGatewayEventError exn ->
        eprintfn "%A" exn
        model, Cmd.none

let subscribe model =
    []
