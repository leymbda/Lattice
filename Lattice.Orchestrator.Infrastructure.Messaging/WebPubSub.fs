module Lattice.Orchestrator.Infrastructure.Messaging.WebPubSub

open Azure.Core
open Azure.Messaging.WebPubSub
open Lattice.Orchestrator.Contracts
open System.Threading.Tasks
open Thoth.Json.Net

let shardInstanceScheduleStart (client: WebPubSubServiceClient) nodeId shardId token intents handler startAt = task {
    let message =
        {
            NodeId = nodeId
            ShardId = shardId
            Token = token
            Intents = intents
            Handler = handler
            StartAt = startAt
        }
        |> ShardInstanceSendScheduleStartMessage.encoder
        |> Encode.toString 0

    do! client.SendToUserAsync(nodeId.ToString(), message, ContentType.ApplicationJson) |> Task.wait
    // TODO: How to define event as "shard-instance-schedule-starts"
}

let shardInstanceScheduleClose (client: WebPubSubServiceClient) nodeId shardId closeAt = task {
    let message =
        {
            NodeId = nodeId
            ShardId = shardId
            CloseAt = closeAt
        }
        |> ShardInstanceSendScheduleCloseMessage.encoder
        |> Encode.toString 0

    do! client.SendToUserAsync(nodeId.ToString(), message, ContentType.ApplicationJson) |> Task.wait
    // TODO: How to define event as "shard-instance-schedule-closes"
}

let shardInstanceGatewayEvent (client: WebPubSubServiceClient) nodeId shardId event = task {
    let message =
        {
            NodeId = nodeId
            ShardId = shardId
            Event = event
        }
        |> ShardInstanceSendGatewayEventMessage.encoder
        |> Encode.toString 0

    do! client.SendToUserAsync(nodeId.ToString(), message, ContentType.ApplicationJson) |> Task.wait
    // TODO: How to define event as "shard-instance-gateway-events"
}
