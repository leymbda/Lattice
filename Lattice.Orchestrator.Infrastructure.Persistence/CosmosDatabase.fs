module Lattice.Orchestrator.Infrastructure.Persistence.CosmosDatabase

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Cosmos
open System.Threading.Tasks

let [<Literal>] COSMOS_DATABASE_NAME = "lattice-db"
let [<Literal>] APPLICATION_CONTAINER_NAME = "applications"
let [<Literal>] SHARD_CONTAINER_NAME = "shards"

let getApplicationContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, APPLICATION_CONTAINER_NAME)

let getShardContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, SHARD_CONTAINER_NAME)

let getApplicationById (cosmosClient: CosmosClient): GetApplicationById = fun id -> task {
    let container = getApplicationContainer cosmosClient

    try
        let! res = container.ReadItemAsync<ApplicationModel>(id, PartitionKey id)
        return res.Resource |> ApplicationMapper.toDomain |> Ok
    with | _ ->
        return Error ()
}

let upsertApplication (cosmosClient: CosmosClient): UpsertApplication = fun application -> task {
    let container = getApplicationContainer cosmosClient

    let id = application |> function
        | Application.REGISTERED app -> app.Id
        | Application.ACTIVATED app -> app.Id

    try
        let! res = container.UpsertItemAsync<ApplicationModel>(ApplicationMapper.fromDomain application, PartitionKey id)
        return res.Resource |> ApplicationMapper.toDomain |> Ok
    with | _ ->
        return Error ()
}

let deleteApplicationById (cosmosClient: CosmosClient): DeleteApplicationById = fun id -> task {
    let container = getApplicationContainer cosmosClient

    try
        do! container.DeleteItemAsync<ApplicationModel>(id, PartitionKey id) :> Task
        return Ok ()
    with | _ ->
        return Error ()
}

let getShardById (cosmosClient: CosmosClient): GetShardById = fun id -> task {
    let container = getShardContainer cosmosClient

    try
        let! res = container.ReadItemAsync<ShardModel>(id, PartitionKey id)
        return res.Resource |> ShardMapper.toDomain |> Ok
    with | _ ->
        return Error ()
}

let getShardsByApplicationId (cosmosClient: CosmosClient): GetShardsByApplicationId = fun applicationId -> task {
    let container = getShardContainer cosmosClient

    try
        let query = $"SELECT * FROM c WHERE c.applicationId = '{applicationId}'"
        let iterator = container.GetItemQueryIterator<ShardModel>(query)

        let rec loop (iterator: FeedIterator<ShardModel>) results = task {
            match iterator.HasMoreResults with
            | false -> return results
            | true ->
                let! res = iterator.ReadNextAsync()
                let items = res.Resource |> Seq.toList |> List.map ShardMapper.toDomain
                return! loop iterator (results @ items)
        }

        let! shards = loop iterator []
        return Ok shards
    with | _ ->
        return Error ()
}

let upsertShard (cosmosClient: CosmosClient): UpsertShard = fun shard -> task {
    let container = getShardContainer cosmosClient
    let id = shard |> function
        | Shard.BIDDING shard -> shard.Id
        | Shard.PURCHASED shard -> shard.Id
        | Shard.ACTIVE shard -> shard.Id

    try
        let! res = container.UpsertItemAsync<ShardModel>(ShardMapper.fromDomain shard, PartitionKey id)
        return res.Resource |> ShardMapper.toDomain |> Ok
    with | _ ->
        return Error ()
}

let deleteShardById (cosmosClient: CosmosClient): DeleteShardById = fun id -> task {
    let container = getShardContainer cosmosClient

    try
        do! container.DeleteItemAsync<ShardModel>(id, PartitionKey id) :> Task
        return Ok ()
    with | _ ->
        return Error ()
}
