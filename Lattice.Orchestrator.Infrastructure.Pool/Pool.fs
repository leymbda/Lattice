module Lattice.Orchestrator.Infrastructure.Pool.Pool

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Contracts
open Thoth.Json.Net

let shardInstanceScheduleStart (client: ServiceBusClient) (message: ShardInstanceSendScheduleStartMessage) =
    message
    |> ShardInstanceSendScheduleStartMessage.encoder
    |> Encode.toString 0
    |> ServiceBusMessage
    |> client.CreateSender(PoolInboundMessage.queueName).SendMessageAsync

let shardInstanceScheduleClose (client: ServiceBusClient) (message: ShardInstanceSendScheduleCloseMessage) =
    message
    |> ShardInstanceSendScheduleCloseMessage.encoder
    |> Encode.toString 0
    |> ServiceBusMessage
    |> client.CreateSender(PoolInboundMessage.queueName).SendMessageAsync

let shardInstanceGatewayEvent (client: ServiceBusClient) (message: ShardInstanceSendGatewayEventMessage) =
    message
    |> ShardInstanceSendGatewayEventMessage.encoder
    |> Encode.toString 0
    |> ServiceBusMessage
    |> client.CreateSender(PoolInboundMessage.queueName).SendMessageAsync
