module Lattice.Orchestrator.Infrastructure.Persistence.Cosmos

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Cosmos
open System.Threading.Tasks

let [<Literal>] COSMOS_DATABASE_NAME = "lattice-db"
let [<Literal>] USER_CONTAINER_NAME = "users"
let [<Literal>] APPLICATION_CONTAINER_NAME = "applications"
let [<Literal>] TEAM_CACHE_CONTAINER_NAME = "team-cache" // TODO: Remember to set TTL on cache

let getUserContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, USER_CONTAINER_NAME)

let getApplicationContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, APPLICATION_CONTAINER_NAME)

let getTeamCacheContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, APPLICATION_CONTAINER_NAME)

let upsertUser (cosmosClient) (user: Lattice.Orchestrator.Domain.User) = task {
    let container = getUserContainer cosmosClient

    try
        let! res = container.UpsertItemAsync<UserModel>(UserModel.fromDomain user, PartitionKey user.Id)
        return res.Resource |> UserModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

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

let getCachedApplicationTeam (cosmosClient: CosmosClient) applicationId = task {
    let container = getTeamCacheContainer cosmosClient

    try
        let! res = container.ReadItemAsync<TeamCacheModel>(applicationId, PartitionKey applicationId)
        return res.Resource |> TeamCacheModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let upsertCachedApplicationTeam (cosmosClient: CosmosClient) (team: Team) = task {
    let container = getTeamCacheContainer cosmosClient
    
    try
        let! res = container.UpsertItemAsync<TeamCacheModel>(TeamCacheModel.fromDomain team, PartitionKey team.ApplicationId)
        return res.Resource |> TeamCacheModel.toDomain |> Ok
    with | _ ->
        return Error ()
}

let deleteCachedApplicationTeam (cosmosClient: CosmosClient) applicationId = task {
    let container = getTeamCacheContainer cosmosClient

    try
        do! container.DeleteItemAsync<TeamCacheModel>(applicationId, PartitionKey applicationId) :> Task
        return Ok ()
    with | _ ->
        return Error ()
}
