namespace Lattice.Orchestrator.Domain

open System

type Shard =
    | BIDDING   of BiddingShard
    | PURCHASED of PurchasedShard
    | ACTIVE    of ActiveShard

and BiddingShard = {
    Id:                   string
    ApplicationId:        string
    ShardUnit:            (int * int)
    GreatestBid:          int
    GreatestBidderNodeId: string
    BiddingExpiry:        DateTime
}

and PurchasedShard = {
    Id:               string
    ApplicationId:    string
    ShardUnit:        (int * int)
    NodeId:           string
    ActivationExpiry: DateTime
}

and ActiveShard = {
    Id:                string
    ApplicationId:     string
    ShardUnit:         (int * int)
    NodeId:            string
    GatewayResumeUrl:  string
    GatewaySequenceId: int
    LastHeartbeat:     DateTime
}
