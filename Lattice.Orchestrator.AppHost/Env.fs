namespace Lattice.Orchestrator.AppHost

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Persistence
open Lattice.Orchestrator.Infrastructure.Pool
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
            httpClientFactory.CreateOAuthClient accessToken
            |> Rest.getCurrentUser
            |> Task.map (fst >> Result.toOption)

    interface IPool with
        member _.ShardInstanceScheduleStart nodeId shardId token intents handler startAt =
            Pool.shardInstanceScheduleStart serviceBusClient {
                NodeId = nodeId
                ShardId = shardId
                Token = token
                Intents = intents
                Handler = handler
                StartAt = startAt
            }

        member _.ShardInstanceScheduleClose nodeId shardId closeAt =
            Pool.shardInstanceScheduleClose serviceBusClient {
                NodeId = nodeId
                ShardId = shardId
                CloseAt = closeAt
            }

        member _.ShardInstanceGatewayEvent nodeId shardId event =
            Pool.shardInstanceGatewayEvent serviceBusClient {
                NodeId = nodeId
                ShardId = shardId
                Event = event
            }
    
    interface IPersistence with
        member _.SetUser user = Cosmos.setUser cosmosClient user

        member _.GetApp appId = Cosmos.getApp cosmosClient appId
        member _.SetApp app = Cosmos.setApp cosmosClient app
        member _.RemoveApp appId = Cosmos.removeApp cosmosClient appId

    interface ISecrets with
        member _.BotTokenEncryptionKey = configuration.GetValue<string> "BotTokenEncryptionKey"
