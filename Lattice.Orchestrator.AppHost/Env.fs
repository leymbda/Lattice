namespace Lattice.Orchestrator.AppHost

open Azure.Messaging.WebPubSub
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
    webPubSubClient: WebPubSubServiceClient
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
            httpClientFactory.CreateOAuthClient accessToken
            |> Rest.getCurrentUser
            |> Task.map (fst >> Result.toOption)

    interface IEvents with
        member _.ShardInstanceScheduleStart (nodeId, shardId, token, intents, handler, startAt) = WebPubSub.shardInstanceScheduleStart webPubSubClient nodeId shardId token intents handler startAt
        member _.ShardInstanceScheduleClose (nodeId, shardId, closeAt) = WebPubSub.shardInstanceScheduleClose webPubSubClient nodeId shardId closeAt
        member _.ShardInstanceGatewayEvent (nodeId, shardId, event) = WebPubSub.shardInstanceGatewayEvent webPubSubClient nodeId shardId event
    
    interface IPersistence with
        member _.SetUser user = Cosmos.setUser cosmosClient user

        member _.GetApp appId = Cosmos.getApp cosmosClient appId
        member _.SetApp app = Cosmos.setApp cosmosClient app
        member _.RemoveApp appId = Cosmos.removeApp cosmosClient appId

    interface ISecrets with
        member _.BotTokenEncryptionKey = configuration.GetValue<string> "BotTokenEncryptionKey"
