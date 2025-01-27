module Lattice.Orchestrator.Infrastructure.Persistence.Cosmos

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Cosmos
open System
open System.Threading.Tasks

let [<Literal>] COSMOS_DATABASE_NAME = "lattice-db"
let [<Literal>] APPLICATION_CONTAINER_NAME = "applications"
let [<Literal>] NODE_CONTAINER_NAME = "nodes"
let [<Literal>] SHARD_CONTAINER_NAME = "shards"

let getApplicationContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, APPLICATION_CONTAINER_NAME)

let getNodeContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, NODE_CONTAINER_NAME)

let getShardContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, SHARD_CONTAINER_NAME)

let getApplicationById (cosmosClient: CosmosClient) id = task {
    let container = getApplicationContainer cosmosClient

    try
        let! res = container.ReadItemAsync<ApplicationModel>(id, PartitionKey id)
        return res.Resource |> ApplicationModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let upsertApplication (cosmosClient: CosmosClient) application = task {
    let container = getApplicationContainer cosmosClient

    try
        let! res = container.UpsertItemAsync<ApplicationModel>(ApplicationModel.fromDomain application, PartitionKey application.Id)
        return res.Resource |> ApplicationModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let deleteApplicationById (cosmosClient: CosmosClient) id = task {
    let container = getApplicationContainer cosmosClient

    try
        do! container.DeleteItemAsync<ApplicationModel>(id, PartitionKey id) :> Task
        return Ok ()
    with | _ ->
        return Error ()
}

let getNodeById (cosmosClient: CosmosClient) (id: Guid) = task {
    let container = getNodeContainer cosmosClient

    let idStr = id.ToString()

    try
        let! res = container.ReadItemAsync<NodeModel>(idStr, PartitionKey idStr)
        return res.Resource |> NodeModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let upsertNode (cosmosClient: CosmosClient) node = task {
    let container = getNodeContainer cosmosClient

    try
        let! res = container.UpsertItemAsync<NodeModel>(NodeModel.fromDomain node, PartitionKey (node.Id.ToString()))
        return res.Resource |> NodeModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let deleteNodeById (cosmosClient: CosmosClient) id = task {
    let container = getNodeContainer cosmosClient

    try
        do! container.DeleteItemAsync<NodeModel>(id, PartitionKey id) :> Task
        return Ok ()
    with | _ ->
        return Error ()
}

let getShardById (cosmosClient: CosmosClient) id = task {
    let container = getShardContainer cosmosClient

    try
        let! res = container.ReadItemAsync<ShardModel>(id, PartitionKey id)
        return res.Resource |> ShardModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let getShardsByApplicationId (cosmosClient: CosmosClient) id = task {
    let container = getShardContainer cosmosClient

    try
        let query = $"SELECT * FROM c WHERE c.applicationId = '{id}'"
        let iterator = container.GetItemQueryIterator<ShardModel>(query)

        let mutable items = List.empty<ShardModel>

        while iterator.HasMoreResults do
            let! res = iterator.ReadNextAsync()
            items <- items @ (res.Resource |> Seq.toList)

        return Ok (items |> List.map ShardModel.toDomain)
    with | _ ->
        return Error ()
}

let upsertShard (cosmosClient: CosmosClient) shard = task {
    let container = getShardContainer cosmosClient

    let id = shard |> function
        | Shard.BIDDING shard -> shard.Id
        | Shard.PURCHASED shard -> shard.Id
        | Shard.ACTIVE shard -> shard.Id

    try
        let! res = container.UpsertItemAsync<ShardModel>(ShardModel.fromDomain shard, PartitionKey id)
        return res.Resource |> ShardModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let deleteShardById (cosmosClient: CosmosClient) id = task {
    let container = getShardContainer cosmosClient

    try
        do! container.DeleteItemAsync<ShardModel>(id, PartitionKey id) :> Task
        return Ok ()
    with | _ ->
        return Error ()
}
