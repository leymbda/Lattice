namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type PoolInboundMessage =
    | ShardInstanceScheduleStart of ShardInstanceSendScheduleStartMessage
    | ShardInstanceScheduleClose of ShardInstanceSendScheduleCloseMessage
    | ShardInstanceGatewayEvent  of ShardInstanceSendGatewayEventMessage

module PoolInboundMessage =
    let decoder: Decoder<PoolInboundMessage> =
        Decode.oneOf [
            Decode.map PoolInboundMessage.ShardInstanceScheduleStart ShardInstanceSendScheduleStartMessage.decoder
            Decode.map PoolInboundMessage.ShardInstanceScheduleClose ShardInstanceSendScheduleCloseMessage.decoder
            Decode.map PoolInboundMessage.ShardInstanceGatewayEvent ShardInstanceSendGatewayEventMessage.decoder
        ]

    let encoder (v: PoolInboundMessage) =
        match v with
        | ShardInstanceScheduleStart m -> ShardInstanceSendScheduleStartMessage.encoder m
        | ShardInstanceScheduleClose m -> ShardInstanceSendScheduleCloseMessage.encoder m
        | ShardInstanceGatewayEvent m -> ShardInstanceSendGatewayEventMessage.encoder m
        
    let [<Literal>] queueName = "poolinbound"
