namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.EventGrid
open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Discord
open Lattice.Orchestrator.Infrastructure.Messaging
open Lattice.Orchestrator.Infrastructure.Persistence
open FSharp.Discord.Rest
open Microsoft.Azure.Cosmos

type IEnv =
    inherit IDiscord
    inherit IEvents
    inherit IPersistence
    inherit ISecrets

type Env (
    secrets: SecretsOptions,
    discordClientFactory: IDiscordClientFactory,
    cosmosClient: CosmosClient,
    eventGridPublisherClient: EventGridPublisherClient,
    serviceBusClient: ServiceBusClient
) =
    interface IEnv

    interface IDiscord with
        member _.GetApplicationInformation botToken = Discord.getApplicationInformation discordClientFactory botToken
        member _.GetUserInformation accessToken = Discord.getUserInformation discordClientFactory accessToken
        member _.ExchangeCodeForAccessToken redirectUri code = Discord.exchangeCodeForAccessToken discordClientFactory secrets.DiscordClientId secrets.DiscordClientSecret redirectUri code

    interface IEvents with
        member _.NodeHeartbeat nodeId heartbeatTime = EventGrid.nodeHeartbeat eventGridPublisherClient nodeId heartbeatTime
        member _.NodeRelease nodeId = ServiceBus.nodeRelease serviceBusClient nodeId
        member _.NodeRedistribute nodeId = ServiceBus.nodeRedistribute serviceBusClient nodeId
    
    interface IPersistence with
        member _.UpsertUser user = Cosmos.upsertUser cosmosClient user

        member _.GetApplicationById id = Cosmos.getApplicationById cosmosClient id
        member _.UpsertApplication application = Cosmos.upsertApplication cosmosClient application
        member _.DeleteApplicationById id = Cosmos.deleteApplicationById cosmosClient id

        member _.GetNodeById id = Cosmos.getNodeById cosmosClient id
        member _.UpsertNode node = Cosmos.upsertNode cosmosClient node
        member _.DeleteNodeById id = Cosmos.deleteNodeById cosmosClient id

        member _.GetShardById id = Cosmos.getShardById cosmosClient id
        member _.GetShardsByApplicationId id = Cosmos.getShardsByApplicationId cosmosClient id
        member _.UpsertShard shard = Cosmos.upsertShard cosmosClient shard
        member _.DeleteShardById id = Cosmos.deleteShardById cosmosClient id
        
    interface ISecrets with
        member _.ClientId = secrets.DiscordClientId
        member _.ClientSecret = secrets.DiscordClientSecret

        member _.UserAccessTokenEncryptionKey = secrets.UserAccessTokenEncryptionKey
        member _.UserRefreshTokenEncryptionKey = secrets.UserRefreshTokenEncryptionKey
        member _.BotTokenEncryptionKey = secrets.BotTokenEncryptionKey

        member _.JwtHashingKey = secrets.JwtHashingKey
