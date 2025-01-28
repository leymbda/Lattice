namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Discord
open Lattice.Orchestrator.Infrastructure.Persistence
open FSharp.Discord.Rest
open Microsoft.Azure.Cosmos
open Microsoft.DurableTask.Client
open System.Net.Http
open Microsoft.DurableTask.Entities

type DiscordClientFactory (httpClientFactory: IHttpClientFactory) =
    interface IDiscordClientFactory with
        member _.CreateBotClient token = httpClientFactory.CreateClient() |> HttpClient.toBotClient token
        member _.CreateOAuthClient token = httpClientFactory.CreateClient() |> HttpClient.toOAuthClient token
        member _.CreateBasicClient clientId clientSecret = httpClientFactory.CreateClient() |> HttpClient.toBasicClient clientId clientSecret

// TODO: Move DiscordClientFactory concrete implementation elsewhere
// TODO: Create client factories for CosmosClient and DurableTaskClient? Or just keep as is? Figure out

type Env (discordClientFactory: IDiscordClientFactory, cosmosClient: CosmosClient, durableTaskClient: DurableTaskClient) =
    interface IEnv

    interface IDiscord with
        member _.GetApplicationInformation token = Discord.getApplicationInformation discordClientFactory token
    
    interface IPersistence with
        member _.GetApplicationById id = Cosmos.getApplicationById cosmosClient id
        member _.UpsertApplication application = Cosmos.upsertApplication cosmosClient application
        member _.DeleteApplicationById id = Cosmos.deleteApplicationById cosmosClient id

        member _.GetNodeShardCounts () = raise (System.NotImplementedException())

        member _.GetNodeById id = Cosmos.getNodeById cosmosClient id
        member _.UpsertNode node = Cosmos.upsertNode cosmosClient node
        member _.DeleteNodeById id = Cosmos.deleteNodeById cosmosClient id

        member _.GetShardById id = Cosmos.getShardById cosmosClient id
        member _.GetShardsByApplicationId id = Cosmos.getShardsByApplicationId cosmosClient id
        member _.UpsertShard shard = Cosmos.upsertShard cosmosClient shard
        member _.DeleteShardById id = Cosmos.deleteShardById cosmosClient id

    interface INodeEntityClient with
        member _.Heartbeat nodeId heartbeatTime =
            durableTaskClient.Entities.SignalEntityAsync(
                EntityInstanceId(nameof NodeEntity, nodeId.ToString()),
                nameof Unchecked.defaultof<NodeEntity>.Heartbeat,
                heartbeatTime)

        member _.Release nodeId =
            durableTaskClient.Entities.SignalEntityAsync(
                EntityInstanceId(nameof NodeEntity, nodeId.ToString()),
                nameof Unchecked.defaultof<NodeEntity>.Release)
            
        member _.Redistribute nodeId =
            durableTaskClient.Entities.SignalEntityAsync(
                EntityInstanceId(nameof NodeEntity, nodeId.ToString()),
                nameof Unchecked.defaultof<NodeEntity>.Redistribute)
            
        // TODO: Thesse should probably be triggering event grid events to run operations instead of calling the entity directly
