module Lattice.Orchestrator.Infrastructure.Persistence.Cosmos

open Lattice.Orchestrator.Domain
open Microsoft.Azure.Cosmos
open System.Threading.Tasks

let [<Literal>] COSMOS_DATABASE_NAME = "lattice-db"
let [<Literal>] USER_CONTAINER_NAME = "users"
let [<Literal>] APPLICATION_CONTAINER_NAME = "applications"

let getUserContainer (cosmosClient: CosmosClient) =
    cosmosClient.GetContainer(COSMOS_DATABASE_NAME, USER_CONTAINER_NAME)

let getApplicationContainer (cosmosClient: CosmosClient) =
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
