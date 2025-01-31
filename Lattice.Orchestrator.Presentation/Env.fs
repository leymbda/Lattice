namespace Lattice.Orchestrator.Presentation

open Azure.Messaging.EventGrid
open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Discord
open Lattice.Orchestrator.Infrastructure.Messaging
open Lattice.Orchestrator.Infrastructure.Persistence
open FSharp.Discord.Rest
open Microsoft.Azure.Cosmos
open Microsoft.Extensions.Configuration

type IEnv =
    inherit IDiscord
    inherit IEvents
    inherit IPersistence
    inherit ISecrets

type Env (
    configuration: IConfiguration,
    discordClientFactory: IDiscordClientFactory,
    cosmosClient: CosmosClient,
    eventGridPublisherClient: EventGridPublisherClient,
    serviceBusClient: ServiceBusClient
) =
    interface IEnv

    interface IDiscord with
        member _.GetApplicationInformation botToken = Discord.getApplicationInformation discordClientFactory botToken
        member _.GetUserInformation accessToken = Discord.getUserInformation discordClientFactory accessToken
        member _.ExchangeCodeForAccessToken redirectUri code =
            Discord.exchangeCodeForAccessToken
                discordClientFactory
                (configuration.GetValue<string>("DiscordClientId"))
                (configuration.GetValue<string>("DiscordClientSecret"))
                redirectUri
                code

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
        member _.ClientId = configuration.GetValue<string>("DiscordClientId")
        member _.ClientSecret = configuration.GetValue<string>("DiscordClientSecret")

        member _.UserAccessTokenEncryptionKey = configuration.GetValue<string>("UserAccessTokenEncryptionKey")
        member _.UserRefreshTokenEncryptionKey = configuration.GetValue<string>("UserRefreshTokenEncryptionKey")
        member _.BotTokenEncryptionKey = configuration.GetValue<string>("BotTokenEncryptionKey")

        member _.JwtHashingKey = configuration.GetValue<string>("JwtHashingKey")

    // TODO: Create options instead of accessing configuration directly (which will also clean up IDiscord.ExchangeCodeForAccessToken) 
