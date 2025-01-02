module Lattice.Orchestrator.Application.Shard

open Lattice.Orchestrator.Domain
open System

let auction id applicationId shardUnit biddingExpiry =
    {
        Id = id
        ApplicationId = applicationId
        ShardUnit = shardUnit
        GreatestBid = None
        BiddingExpiry = biddingExpiry
    }

let bid nodeId amount (shard: BiddingShard) =
    let bid = { NodeId = nodeId; Amount = amount }

    match shard.GreatestBid with
    | Some { Amount = currentAmount } when amount > currentAmount -> { shard with GreatestBid = Some bid }
    | None -> { shard with GreatestBid = Some bid }
    | _ -> shard

let isBiddingExpired (time: DateTime) (shard: BiddingShard) =
    time > shard.BiddingExpiry

let purchase activationExpiry (shard: BiddingShard) =
    match shard with
    | { GreatestBid = Some bid } ->
        Some {
            Id = shard.Id
            ApplicationId = shard.ApplicationId
            ShardUnit = shard.ShardUnit
            NodeId = bid.NodeId
            ActivationExpiry = activationExpiry
        }
    | _ -> None

let activate gatewayResumeUrl gatewaySequenceId lastHeartbeat (shard: PurchasedShard) =
    {
        Id = shard.Id
        ApplicationId = shard.ApplicationId
        ShardUnit = shard.ShardUnit
        NodeId = shard.NodeId
        GatewayResumeUrl = gatewayResumeUrl
        GatewaySequenceId = gatewaySequenceId
        LastHeartbeat = lastHeartbeat
    }

let heartbeat lastHeartbeat (shard: ActiveShard) =
    { shard with LastHeartbeat = lastHeartbeat }

let isHeartbeatStale (time: DateTime) (shard: ActiveShard) =
    let delta = time - shard.LastHeartbeat
    let secondsToStale = SHARD_HEARTBEAT_INTERVAL * 2 |> float

    delta > TimeSpan.FromSeconds secondsToStale
