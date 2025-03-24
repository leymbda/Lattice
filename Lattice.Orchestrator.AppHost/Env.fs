namespace Lattice.Orchestrator.AppHost

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Messaging
open Lattice.Orchestrator.Infrastructure.Persistence
open FSharp.Discord.Rest.Old
open Microsoft.Azure.Cosmos
open Microsoft.Extensions.Configuration
open System.Net.Http
open System.Threading.Tasks

type Env (
    configuration: IConfiguration,
    httpClientFactory: IHttpClientFactory,
    cosmosClient: CosmosClient,
    serviceBusClient: ServiceBusClient
) =
    interface IEnv
    
    interface ICache with
        member _.GetTeam applicationId = Cosmos.getTeam cosmosClient applicationId
        member _.SetTeam team = Cosmos.setTeam cosmosClient team
        member _.RemoveTeam applicationId = Cosmos.removeTeam cosmosClient applicationId

    interface IDiscord with
        member _.GetApplicationInformation botToken =
            httpClientFactory.CreateClient()
            |> HttpClient.toBotClient botToken
            |> Rest.getCurrentApplication
            |> Task.map (Result.toOption >> (Option.map _.Data))

        member _.GetUserInformation accessToken =
            httpClientFactory.CreateClient()
            |> HttpClient.toOAuthClient accessToken
            |> Rest.getCurrentUser
            |> Task.map (Result.toOption >> (Option.map _.Data))

        // TODO: Replace with new rest functions once created (will make these neater/simpler in the process)

    interface IEvents with
        member _.NodeHeartbeat nodeId heartbeatTime = ServiceBus.nodeHeartbeat serviceBusClient nodeId heartbeatTime
        member _.NodeRelease nodeId = ServiceBus.nodeRelease serviceBusClient nodeId
        member _.NodeRedistribute nodeId = ServiceBus.nodeRedistribute serviceBusClient nodeId
    
    interface IPersistence with
        member _.UpsertUser user = Cosmos.upsertUser cosmosClient user

        member _.GetApplicationById id = Cosmos.getApplicationById cosmosClient id
        member _.UpsertApplication application = Cosmos.upsertApplication cosmosClient application
        member _.DeleteApplicationById id = Cosmos.deleteApplicationById cosmosClient id

    interface ISecrets with
        member _.BotTokenEncryptionKey = configuration.GetValue<string> "BotTokenEncryptionKey"
