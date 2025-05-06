namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type PoolOutboundMessage =
    | NodeConnected             of NodeConnectedMessage
    | NodeDisconnected          of NodeDisconnectedMessage
    | ShardIrrecoverableClosure of ShardReceiveIrrecoverableClosureMessage
    | NodeHeartbeat             of NodeReceiveHeartbeatMessage
    | NodeScheduleShutdown      of NodeReceiveScheduleShutdownMessage

module PoolOutboundMessage =
    let decoder: Decoder<PoolOutboundMessage> =
        Decode.oneOf [
            Decode.map PoolOutboundMessage.NodeConnected NodeConnectedMessage.decoder
            Decode.map PoolOutboundMessage.NodeDisconnected NodeDisconnectedMessage.decoder
            Decode.map PoolOutboundMessage.ShardIrrecoverableClosure ShardReceiveIrrecoverableClosureMessage.decoder
            Decode.map PoolOutboundMessage.NodeHeartbeat NodeReceiveHeartbeatMessage.decoder
            Decode.map PoolOutboundMessage.NodeScheduleShutdown NodeReceiveScheduleShutdownMessage.decoder
        ]

    let encoder (v: PoolOutboundMessage) =
        match v with
        | PoolOutboundMessage.NodeConnected m -> NodeConnectedMessage.encoder m
        | PoolOutboundMessage.NodeDisconnected m -> NodeDisconnectedMessage.encoder m
        | PoolOutboundMessage.ShardIrrecoverableClosure m -> ShardReceiveIrrecoverableClosureMessage.encoder m
        | PoolOutboundMessage.NodeHeartbeat m -> NodeReceiveHeartbeatMessage.encoder m
        | PoolOutboundMessage.NodeScheduleShutdown m -> NodeReceiveScheduleShutdownMessage.encoder m

    let [<Literal>] queueName = "pooloutbound"
