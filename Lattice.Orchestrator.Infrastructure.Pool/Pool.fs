module Lattice.Orchestrator.Infrastructure.Pool.Pool

open Azure.Messaging.WebPubSub
open Lattice.Orchestrator.Contracts
open Thoth.Json.Net

let shardInstanceScheduleStart (client: WebPubSubServiceClient) (message: ShardInstanceSendScheduleStartMessage) =
    let content =
        message
        |> ShardInstanceSendScheduleStartMessage.encoder
        |> Encode.toString 0

    client.SendToUserAsync(message.NodeId.ToString(), content)

let shardInstanceScheduleClose (client: WebPubSubServiceClient) (message: ShardInstanceSendScheduleCloseMessage) =
    let content =
        message
        |> ShardInstanceSendScheduleCloseMessage.encoder
        |> Encode.toString 0

    client.SendToUserAsync(message.NodeId.ToString(), content)

let shardInstanceGatewayEvent (client: WebPubSubServiceClient) (message: ShardInstanceSendGatewayEventMessage) =
    let content =
        message
        |> ShardInstanceSendGatewayEventMessage.encoder
        |> Encode.toString 0

    client.SendToUserAsync(message.NodeId.ToString(), content)
