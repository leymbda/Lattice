module Lattice.WorkerNode.Shard

open Elmish
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain

type Model = {
    Id: ShardId
}

[<RequireQualifiedAccess>]
type Msg =
    | Connect
    | OnConnectSuccess of unit
    | OnConnectError of exn

    | SendGatewayEvent of GatewaySendEvent
    | OnSendGatewayEventError of exn

    | ReceiveGatewayEvent of GatewayReceiveEvent
    | OnReceiveGatewayEventError of exn

    // TODO: Shard irrecoverable disconnect

/// Initiate connection to the gateway
let private connect model () = async {
    return () // TODO: Implement
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
        model, Cmd.OfAsync.either (connect model) () Msg.OnConnectSuccess Msg.OnConnectError

    | Msg.OnConnectSuccess _ ->
        model, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
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
