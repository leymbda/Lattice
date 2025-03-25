namespace Lattice.Orchestrator.AppHost

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Messaging
open Lattice.Orchestrator.Infrastructure.Persistence
open FSharp.Discord.Rest
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
        member _.GetTeam appId = Cosmos.getTeam cosmosClient appId
        member _.SetTeam team = Cosmos.setTeam cosmosClient team
        member _.RemoveTeam appId = Cosmos.removeTeam cosmosClient appId

    interface IDiscord with
        member _.GetApplicationInformation botToken =
            httpClientFactory.CreateBotClient botToken
            |> Rest.getCurrentApplication
            |> Task.map (fst >> Result.toOption)

        member _.GetUserInformation accessToken =
            httpClientFactory.CreateClient()
            |> FSharp.Discord.Rest.Old.HttpClient.toOAuthClient accessToken
            |> FSharp.Discord.Rest.Old.Rest.getCurrentUser
            |> Task.map (Result.toOption >> (Option.map _.Data))

            // TODO: Replace with new rest once implemented in FSharp.Discord

    interface IEvents with
        member _.NodeHeartbeat nodeId heartbeatTime = ServiceBus.nodeHeartbeat serviceBusClient nodeId heartbeatTime
        member _.NodeRelease nodeId = ServiceBus.nodeRelease serviceBusClient nodeId
        member _.NodeRedistribute nodeId = ServiceBus.nodeRedistribute serviceBusClient nodeId
    
    interface IPersistence with
        member _.SetUser user = Cosmos.setUser cosmosClient user

        member _.GetApp appId = Cosmos.getApp cosmosClient appId
        member _.SetApp app = Cosmos.setApp cosmosClient app
        member _.RemoveApp appId = Cosmos.removeApp cosmosClient appId

    interface ISecrets with
        member _.BotTokenEncryptionKey = configuration.GetValue<string> "BotTokenEncryptionKey"
