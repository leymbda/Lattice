namespace Lattice.Orchestrator.Infrastructure.Persistence

open System.Text.Json
open System.Text.Json.Serialization

type ShardModelType =
    | BIDDING   = 0
    | PURCHASED = 1
    | ACTIVE    = 2

and BiddingShardModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "applicationId">] ApplicationId: string
    [<JsonPropertyName "type">] Type: ShardModelType
    [<JsonPropertyName "shardNumber">] ShardNumber: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "greatestBidNodeId">] GreatestBidNodeId: string option
    [<JsonPropertyName "greatestBigAmount">] GreatestBidAmount: int option
    [<JsonPropertyName "biddingExpiry">] BiddingExpiry: int
}

and PurchasedShardModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "applicationId">] ApplicationId: string
    [<JsonPropertyName "type">] Type: ShardModelType
    [<JsonPropertyName "shardNumber">] ShardNumber: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "nodeId">] NodeId: string
    [<JsonPropertyName "activationExpiry">] ActivationExpiry: int
}

and ActiveShardModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "applicationId">] ApplicationId: string
    [<JsonPropertyName "type">] Type: ShardModelType
    [<JsonPropertyName "shardNumber">] ShardNumber: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "nodeId">] NodeId: string
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
