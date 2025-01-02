module Lattice.Orchestrator.Infrastructure.Persistence.ShardMapper

open Lattice.Orchestrator.Domain
open System

let toDomain model =
    match model with
    | ShardModel.BIDDING model ->
        Shard.BIDDING {
            Id = model.Id
            ApplicationId = model.ApplicationId
            ShardUnit = (model.ShardNumber, model.ShardCount)
            GreatestBid =
                match model.GreatestBidNodeId, model.GreatestBidAmount with
                | Some nodeId, Some amount -> Some { NodeId = nodeId; Amount = amount }
                | _ -> None
            BiddingExpiry = DateTime.UnixEpoch.AddSeconds model.BiddingExpiry
        }

    | ShardModel.PURCHASED model ->
        Shard.PURCHASED {
            Id = model.Id
            ApplicationId = model.ApplicationId
            ShardUnit = (model.ShardNumber, model.ShardCount)
            NodeId = model.NodeId
            ActivationExpiry = DateTime.UnixEpoch.AddSeconds model.ActivationExpiry
        }

    | ShardModel.ACTIVE model ->
        Shard.ACTIVE {
            Id = model.Id
            ApplicationId = model.ApplicationId
            ShardUnit = (model.ShardNumber, model.ShardCount)
            NodeId = model.NodeId
            GatewayResumeUrl = model.GatewayResumeUrl
            GatewaySequenceId = model.GatewaySequenceId
            LastHeartbeat = DateTime.UnixEpoch.AddSeconds model.LastHeartbeat
        }

let fromDomain shard =
    match shard with
    | Shard.BIDDING shard ->
        ShardModel.BIDDING {
            Id = shard.Id
            ApplicationId = shard.ApplicationId
            Type = ShardModelType.BIDDING
            ShardNumber = fst shard.ShardUnit
            ShardCount = snd shard.ShardUnit
            GreatestBidNodeId = Option.map (fun bid -> bid.NodeId) shard.GreatestBid
            GreatestBidAmount = Option.map (fun bid -> bid.Amount) shard.GreatestBid
            BiddingExpiry = int (shard.BiddingExpiry - DateTime.UnixEpoch).TotalSeconds
        }

    | Shard.PURCHASED shard ->
        ShardModel.PURCHASED {
            Id = shard.Id
            ApplicationId = shard.ApplicationId
            Type = ShardModelType.PURCHASED
            ShardNumber = fst shard.ShardUnit
            ShardCount = snd shard.ShardUnit
            NodeId = shard.NodeId
            ActivationExpiry = int (shard.ActivationExpiry - DateTime.UnixEpoch).TotalSeconds
        }

    | Shard.ACTIVE shard ->
        ShardModel.ACTIVE {
            Id = shard.Id
            ApplicationId = shard.ApplicationId
            Type = ShardModelType.ACTIVE
            ShardNumber = fst shard.ShardUnit
            ShardCount = snd shard.ShardUnit
            NodeId = shard.NodeId
            GatewayResumeUrl = shard.GatewayResumeUrl
            GatewaySequenceId = shard.GatewaySequenceId
            LastHeartbeat = int (shard.LastHeartbeat - DateTime.UnixEpoch).TotalSeconds
        }
