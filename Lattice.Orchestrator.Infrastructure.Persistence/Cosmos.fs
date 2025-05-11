module Lattice.Orchestrator.Infrastructure.Persistence.Cosmos

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Cosmos
open System.Threading.Tasks

let [<Literal>] COSMOS_DATABASE_NAME = "lattice-db"
let [<Literal>] USER_CONTAINER_NAME = "users"
let [<Literal>] APP_CONTAINER_NAME = "apps"
let [<Literal>] TEAM_CACHE_CONTAINER_NAME = "team-cache" // TODO: Remember to set TTL on cache

type Container =
    | User
    | Application
    | TeamCache

module CosmosClient =
    let getContainer (container: Container) (cosmosClient: CosmosClient) =
        match container with
        | User -> cosmosClient.GetContainer(COSMOS_DATABASE_NAME, USER_CONTAINER_NAME)
        | Application -> cosmosClient.GetContainer(COSMOS_DATABASE_NAME, APP_CONTAINER_NAME)
        | TeamCache -> cosmosClient.GetContainer(COSMOS_DATABASE_NAME, TEAM_CACHE_CONTAINER_NAME)

let setUser (cosmosClient: CosmosClient) (user: Lattice.Orchestrator.Domain.User) = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer User
            |> _.UpsertItemAsync<UserModel>(UserModel.fromDomain user, PartitionKey user.Id)
            |> Task.map (fun res -> res.Resource |> UserModel.toDomain |> Ok)
    with | _ ->
        return Error ()
}

let getApp (cosmosClient: CosmosClient) appId = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer Application
            |> _.ReadItemAsync<AppModel>(appId, PartitionKey appId)
            |> Task.map (fun res -> res.Resource |> AppModel.toDomain |> Ok)
    with | _ ->
        return Error ()
}

let setApp (cosmosClient: CosmosClient) app = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer Application
            |> _.UpsertItemAsync<AppModel>(AppModel.fromDomain app, PartitionKey app.Id)
            |> Task.map (fun res -> res.Resource |> AppModel.toDomain |> Ok)
    with | _ ->
        return Error ()
}

let removeApp (cosmosClient: CosmosClient) id = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer Application
            |> _.DeleteItemAsync<AppModel>(id, PartitionKey id)
            |> Task.map (fun _ -> Ok ())
    with | _ ->
        return Error ()
}

let getTeam (cosmosClient: CosmosClient) appId = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer TeamCache
            |> _.ReadItemAsync<TeamCacheModel>(appId, PartitionKey appId)
            |> Task.map (fun res -> res.Resource |> TeamCacheModel.toDomain |> Ok)
    with | _ ->
        return Error ()
}

let setTeam (cosmosClient: CosmosClient) (team: Team) = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer TeamCache
            |> _.UpsertItemAsync<TeamCacheModel>(TeamCacheModel.fromDomain team, PartitionKey team.AppId)
            |> Task.map (fun res -> res.Resource |> TeamCacheModel.toDomain |> Ok)
    with | _ ->
        return Error ()
}

let removeTeam (cosmosClient: CosmosClient) appId = task {
    try
        return!
            cosmosClient
            |> CosmosClient.getContainer TeamCache
            |> _.DeleteItemAsync<TeamCacheModel>(appId, PartitionKey appId)
            |> Task.map (fun _ -> Ok ())
    with | _ ->
        return Error ()
}
