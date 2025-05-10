module Lattice.WorkerNode.Shard

open Elmish
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain
open System

type DisconnectType =
    | Requested
    | Unexpected
    | Irrecoverable

type Model = {
    Id: ShardId
    Disconnect: (DisconnectType * DateTime option) option
}

[<RequireQualifiedAccess>]
type Msg =
    | Connect
    | OnConnectSuccess
    | OnConnectError of exn

    | Disconnect of type': DisconnectType * shutdownAt: DateTime option
    | OnDisconnect of type': DisconnectType

    | SendGatewayEvent of GatewaySendEvent
    | OnSendGatewayEventError of exn

    | ReceiveGatewayEvent of GatewayReceiveEvent
    | OnReceiveGatewayEventError of exn

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
    {
        Id = id
        Disconnect = None
    },
    Cmd.ofMsg Msg.Connect

let update msg (model: Model) =
    match msg with
    | Msg.Connect ->
        model, Cmd.OfAsync.either (connect model) () (fun _ -> Msg.OnConnectSuccess) Msg.OnConnectError

    | Msg.OnConnectSuccess ->
        printfn "Successfully connected shard %s to the gateway" (ShardId.toString model.Id)
        model, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        { model with Disconnect = Some (DisconnectType.Irrecoverable, None) }, Cmd.none

    | Msg.Disconnect (type', shutdownAt) ->
        { model with Disconnect = Some (type', shutdownAt) }, Cmd.none

    | Msg.OnDisconnect type' ->
        printfn "Disconnected shard %s from the gateway due to %A" (ShardId.toString model.Id) type'
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
    let shutdown =
        match model.Disconnect with
        | None -> []
        | Some (type', shutdownAt) ->
            let timespan =
                shutdownAt
                |> Option.map (_.Subtract(DateTime.UtcNow))
                |> Option.defaultValue (TimeSpan.FromSeconds 0)

            let onShutdown dispatch () =
                // TODO: Disconnect gateway client once implemented
                dispatch (Msg.OnDisconnect type')

            [["shutdown"], fun dispatch -> Sub.delay timespan (onShutdown dispatch)]

    List.empty
    |> List.append shutdown
