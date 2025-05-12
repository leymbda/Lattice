module Lattice.WorkerNode.Shard

open Elmish
open FSharp.Discord.Gateway
open FsToolkit.ErrorHandling
open Lattice.Orchestrator.Domain
open System

type DisconnectType =
    | Requested
    | Unexpected
    | Irrecoverable

type Model = {
    Id: ShardId
    // TODO: Add client and handle its behaviour
    DisabledUntil: DateTime
    DisconnectRequestedFor: DateTime option
    DisconnectType: DisconnectType option
    ConnectedAt: DateTime option
    DisconnectedAt: DateTime option
}

[<RequireQualifiedAccess>]
type Msg =
    | Connect
    | OnConnectSuccess
    | OnConnectError of exn

    | ScheduleDisconnect of type': DisconnectType * shutdownAt: DateTime
    | Disconnect of type': DisconnectType
    | OnDisconnect of type': DisconnectType

    | SendGatewayEvent of GatewaySendEvent
    | OnSendGatewayEventError of exn

let init id disabledUntil =
    {
        Id = id
        DisabledUntil = disabledUntil
        DisconnectRequestedFor = None
        DisconnectType = None
        ConnectedAt = None
        DisconnectedAt = None
    },
    Cmd.ofMsg Msg.Connect

let update msg (model: Model) =
    match msg with
    | Msg.Connect ->
        let connect () =
            asyncResult {
                return () // TODO: Implement
            }
            |> AsyncResult.defaultWith failwith

        model, Cmd.OfAsync.either connect () (fun _ -> Msg.OnConnectSuccess) Msg.OnConnectError

    | Msg.OnConnectSuccess ->
        printfn "Successfully connected shard %s to the gateway" (ShardId.toString model.Id)
        { model with ConnectedAt = Some DateTime.UtcNow }, Cmd.none

    | Msg.OnConnectError exn ->
        eprintfn "%A" exn
        model, Cmd.ofMsg (Msg.Disconnect DisconnectType.Irrecoverable)

    | Msg.ScheduleDisconnect (type', shutdownAt) ->
        { model with
            DisconnectRequestedFor = Some shutdownAt
            DisconnectType = Some type' },
        Cmd.none

    | Msg.Disconnect type' ->
        let disconnect () =
            asyncResult {
                return () // TODO: Implement
            }
            |> AsyncResult.ignoreError

        { model with DisconnectType = Some type' },
        Cmd.OfAsync.perform disconnect () (fun _ -> Msg.OnDisconnect type')

    | Msg.OnDisconnect type' ->
        // TODO: Attempt reconnect if type allows

        printfn "Disconnected shard %s from the gateway due to %A" (ShardId.toString model.Id) type'
        { model with DisconnectedAt = Some DateTime.UtcNow }, Cmd.none

    | Msg.SendGatewayEvent event ->
        let sendGatewayEvent (event: GatewaySendEvent) =
            asyncResult {
                return () // TODO: Implement
            }
            |> AsyncResult.defaultWith failwith

        model, Cmd.OfAsync.attempt sendGatewayEvent event Msg.OnSendGatewayEventError

    | Msg.OnSendGatewayEventError exn ->
        eprintfn "%A" exn
        model, Cmd.none

let subscribe model =
    let shutdown =
        match model.DisconnectRequestedFor with
        | None -> []
        | Some shutdownAt ->
            [["shutdown"], fun dispatch -> Sub.delay (shutdownAt.Subtract DateTime.UtcNow) (fun _ -> dispatch (Msg.Disconnect DisconnectType.Requested))]

    // TODO: Subscribe to if/when the ws itself dies

    Sub.batch [
        shutdown
    ]
