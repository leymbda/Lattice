namespace rec Lattice.Orchestrator.Domain

open System

type BiddingShard = {
    Id:                   string
    ApplicationId:        string
    ShardUnit:            (int * int)
    GreatestBid:          Bid option
    BiddingExpiry:        DateTime
}

module BiddingShard =
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

type PurchasedShard = {
    Id:               string
    ApplicationId:    string
    ShardUnit:        (int * int)
    NodeId:           string
    ActivationExpiry: DateTime
}

module PurchasedShard =
    let activate gatewayResumeUrl gatewaySequenceId lastHeartbeat (shard: PurchasedShard) = {
        Id = shard.Id
        ApplicationId = shard.ApplicationId
        ShardUnit = shard.ShardUnit
        NodeId = shard.NodeId
        GatewayResumeUrl = gatewayResumeUrl
        GatewaySequenceId = gatewaySequenceId
        LastHeartbeat = lastHeartbeat
    }

type ActiveShard = {
    Id:                string
    ApplicationId:     string
    ShardUnit:         (int * int)
    NodeId:            string
    GatewayResumeUrl:  string
    GatewaySequenceId: int
    LastHeartbeat:     DateTime
}

module ActiveShard =
    let [<Literal>] SHARD_HEARTBEAT_INTERVAL = 30

    let heartbeat lastHeartbeat (shard: ActiveShard) =
        { shard with LastHeartbeat = lastHeartbeat }

    let isHeartbeatStale (time: DateTime) (shard: ActiveShard) =
        let delta = time - shard.LastHeartbeat
        let secondsToStale = SHARD_HEARTBEAT_INTERVAL * 2 |> float

        delta > TimeSpan.FromSeconds secondsToStale

type Shard =
    | BIDDING   of BiddingShard
    | PURCHASED of PurchasedShard
    | ACTIVE    of ActiveShard

module Shard =
    let auction id applicationId shardUnit biddingExpiry = {
        Id = id
        ApplicationId = applicationId
        ShardUnit = shardUnit
        GreatestBid = None
        BiddingExpiry = biddingExpiry
    }
