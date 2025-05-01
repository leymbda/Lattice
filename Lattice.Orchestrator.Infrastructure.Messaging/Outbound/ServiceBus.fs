module Lattice.Orchestrator.Infrastructure.Messaging.ServiceBus

open Azure.Messaging.ServiceBus
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
open System
open Thoth.Json.Net

let shardInstanceScheduleStart (client: ServiceBusClient) (nodeId: Guid) (shardId: ShardId) (token: string) (intents: int) (handler: Handler) (startAt: DateTime) =
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
    |> ServiceBusMessage
    |> client.CreateSender("shard-instance-schedule-starts").SendMessageAsync

let shardInstanceScheduleClose (client: ServiceBusClient) (nodeId: Guid) (shardId: ShardId) (closeAt: DateTime) =
    {
        NodeId = nodeId
        ShardId = shardId
        CloseAt = closeAt
    }
    |> ShardInstanceSendScheduleCloseMessage.encoder
    |> Encode.toString 0
    |> ServiceBusMessage
    |> client.CreateSender("shard-instance-schedule-closes").SendMessageAsync

let shardInstanceGatewayEvent (client: ServiceBusClient) (nodeId: Guid) (shardId: ShardId) (event: GatewaySendEvent) =
    {
        NodeId = nodeId
        ShardId = shardId
        Event = event
    }
    |> ShardInstanceSendGatewayEventMessage.encoder
    |> Encode.toString 0
    |> ServiceBusMessage
    |> client.CreateSender("shard-instance-gateway-events").SendMessageAsync
