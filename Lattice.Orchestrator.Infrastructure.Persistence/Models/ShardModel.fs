namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System
open System.Text.Json
open System.Text.Json.Serialization

type ShardModelType =
    | BIDDING   = 0
    | PURCHASED = 1
    | ACTIVE    = 2

and BiddingShardModel = {
    [<JsonPropertyName "id">] Id: Guid
    [<JsonPropertyName "applicationId">] ApplicationId: string
    [<JsonPropertyName "type">] Type: ShardModelType
    [<JsonPropertyName "shardNumber">] ShardNumber: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "greatestBidNodeId">] GreatestBidNodeId: Guid option
    [<JsonPropertyName "greatestBigAmount">] GreatestBidAmount: int option
    [<JsonPropertyName "biddingExpiry">] BiddingExpiry: int
}

and PurchasedShardModel = {
    [<JsonPropertyName "id">] Id: Guid
    [<JsonPropertyName "applicationId">] ApplicationId: string
    [<JsonPropertyName "type">] Type: ShardModelType
    [<JsonPropertyName "shardNumber">] ShardNumber: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "nodeId">] NodeId: Guid
    [<JsonPropertyName "activationExpiry">] ActivationExpiry: int
}

and ActiveShardModel = {
    [<JsonPropertyName "id">] Id: Guid
    [<JsonPropertyName "applicationId">] ApplicationId: string
    [<JsonPropertyName "type">] Type: ShardModelType
    [<JsonPropertyName "shardNumber">] ShardNumber: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "nodeId">] NodeId: Guid
    [<JsonPropertyName "gatewayResumeUrl">] GatewayResumeUrl: string
    [<JsonPropertyName "gatewaySequenceId">] GatewaySequenceId: int
    [<JsonPropertyName "lastHeartbeat">] LastHeartbeat: int
}

[<JsonConverter(typeof<ShardModelConverter>)>]
type ShardModel =
    | BIDDING   of BiddingShardModel
    | PURCHASED of PurchasedShardModel
    | ACTIVE    of ActiveShardModel
    
and ShardModelConverter () =
    inherit JsonConverter<ShardModel>()

    override _.Read (reader, _, _) =
        let success, document = JsonDocument.TryParseValue(&reader)
        if not success then raise (JsonException "Invalid ShardModel provided")

        let applicationType = document.RootElement.GetProperty "type" |> _.GetInt32() |> enum<ShardModelType>
        let json = document.RootElement.GetRawText()

        match applicationType with
        | ShardModelType.BIDDING -> ShardModel.BIDDING <| JsonSerializer.Deserialize<BiddingShardModel> json
        | ShardModelType.PURCHASED -> ShardModel.PURCHASED <| JsonSerializer.Deserialize<PurchasedShardModel> json
        | ShardModelType.ACTIVE -> ShardModel.ACTIVE <| JsonSerializer.Deserialize<ActiveShardModel> json
        | _ -> raise (JsonException "Invalid ShardModel provided")

    override _.Write (writer, value, _) =
        let json =
            match value with
            | ShardModel.BIDDING model -> JsonSerializer.Serialize model
            | ShardModel.PURCHASED model -> JsonSerializer.Serialize model
            | ShardModel.ACTIVE model -> JsonSerializer.Serialize model

        writer.WriteRawValue json

module ShardModel =
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
                GreatestBidNodeId = shard.GreatestBid |> Option.map (fun bid -> bid.NodeId)
                GreatestBidAmount = shard.GreatestBid |> Option.map (fun bid -> bid.Amount)
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
