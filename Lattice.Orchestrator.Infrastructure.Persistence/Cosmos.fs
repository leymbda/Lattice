module Lattice.Orchestrator.Infrastructure.Persistence.Cosmos

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Cosmos
open System.Threading.Tasks

let [<Literal>] COSMOS_DATABASE_NAME = "lattice-db"
let [<Literal>] USER_CONTAINER_NAME = "users"
let [<Literal>] APPLICATION_CONTAINER_NAME = "applications"
let [<Literal>] TEAM_CACHE_CONTAINER_NAME = "team-cache" // TODO: Remember to set TTL on cache

type Container =
    | User
    | Application
    | TeamCache

module CosmosClient =
    let getContainer (container: Container) (cosmosClient: CosmosClient) =
        match container with
        | User -> cosmosClient.GetContainer(COSMOS_DATABASE_NAME, USER_CONTAINER_NAME)
        | Application -> cosmosClient.GetContainer(COSMOS_DATABASE_NAME, APPLICATION_CONTAINER_NAME)
        | TeamCache -> cosmosClient.GetContainer(COSMOS_DATABASE_NAME, TEAM_CACHE_CONTAINER_NAME)

let upsertUser (cosmosClient: CosmosClient) (user: Lattice.Orchestrator.Domain.User) =
    try
        cosmosClient
        |> CosmosClient.getContainer User
        |> _.UpsertItemAsync<UserModel>(UserModel.fromDomain user, PartitionKey user.Id)
        |> Task.map (fun res -> res.Resource |> UserModel.toDomain |> Ok)
    with | _ ->
        Task.FromResult <| Error ()

let getApplicationById (cosmosClient: CosmosClient) id =
    try
        cosmosClient
        |> CosmosClient.getContainer Application
        |> _.ReadItemAsync<ApplicationModel>(id, PartitionKey id)
        |> Task.map (fun res -> res.Resource |> ApplicationModel.toDomain |> Ok)
    with | _ ->
        Task.FromResult <| Error ()

let upsertApplication (cosmosClient: CosmosClient) application =
    try
        cosmosClient
        |> CosmosClient.getContainer Application
        |> _.UpsertItemAsync<ApplicationModel>(ApplicationModel.fromDomain application, PartitionKey application.Id)
        |> Task.map (fun res -> res.Resource |> ApplicationModel.toDomain |> Ok)
    with | _ ->
        Task.FromResult <| Error ()

let deleteApplicationById (cosmosClient: CosmosClient) id =
    try
        cosmosClient
        |> CosmosClient.getContainer Application
        |> _.DeleteItemAsync<ApplicationModel>(id, PartitionKey id)
        |> Task.map (fun _ -> Ok ())
    with | _ ->
        Task.FromResult <| Error ()

let getTeam (cosmosClient: CosmosClient) applicationId =
    try
        cosmosClient
        |> CosmosClient.getContainer TeamCache
        |> _.ReadItemAsync<TeamCacheModel>(applicationId, PartitionKey applicationId)
        |> Task.map (fun res -> res.Resource |> TeamCacheModel.toDomain |> Ok)
    with | _ ->
        Task.FromResult <| Error ()

let setTeam (cosmosClient: CosmosClient) (team: Team) =
    try
        cosmosClient
        |> CosmosClient.getContainer TeamCache
        |> _.UpsertItemAsync<TeamCacheModel>(TeamCacheModel.fromDomain team, PartitionKey team.ApplicationId)
        |> Task.map (fun res -> res.Resource |> TeamCacheModel.toDomain |> Ok)
    with | _ ->
        Task.FromResult <| Error ()

let removeTeam (cosmosClient: CosmosClient) applicationId =
    try
        cosmosClient
        |> CosmosClient.getContainer TeamCache
        |> _.DeleteItemAsync<TeamCacheModel>(applicationId, PartitionKey applicationId)
        |> Task.map (fun _ -> Ok ())
    with | _ ->
        Task.FromResult <| Error ()
