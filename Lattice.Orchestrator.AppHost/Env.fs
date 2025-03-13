namespace Lattice.Orchestrator.AppHost

open Azure.Messaging.ServiceBus
open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Discord
open Lattice.Orchestrator.Infrastructure.Messaging
open Lattice.Orchestrator.Infrastructure.Persistence
open FSharp.Discord.Rest
open Microsoft.Azure.Cosmos
open Microsoft.Extensions.Configuration

type Env (
    configuration: IConfiguration,
    discordClientFactory: IDiscordClientFactory,
    cosmosClient: CosmosClient,
    serviceBusClient: ServiceBusClient
) =
    interface IEnv

    interface IDiscord with
        member _.GetApplicationInformation botToken = Discord.getApplicationInformation discordClientFactory botToken
        member _.GetUserInformation accessToken = Discord.getUserInformation discordClientFactory accessToken

    interface IEvents with
        member _.NodeHeartbeat nodeId heartbeatTime = ServiceBus.nodeHeartbeat serviceBusClient nodeId heartbeatTime
        member _.NodeRelease nodeId = ServiceBus.nodeRelease serviceBusClient nodeId
        member _.NodeRedistribute nodeId = ServiceBus.nodeRedistribute serviceBusClient nodeId
    
    interface IPersistence with
        member _.UpsertUser user = Cosmos.upsertUser cosmosClient user

        member _.GetApplicationById id = Cosmos.getApplicationById cosmosClient id
        member _.UpsertApplication application = Cosmos.upsertApplication cosmosClient application
        member _.DeleteApplicationById id = Cosmos.deleteApplicationById cosmosClient id

        member _.GetCachedApplicationTeam applicationId = Cosmos.getCachedApplicationTeam cosmosClient applicationId
        member _.UpsertCachedApplicationTeam team = Cosmos.upsertCachedApplicationTeam cosmosClient team
        member _.DeleteCachedApplicationTeam applicationId = Cosmos.deleteCachedApplicationTeam cosmosClient applicationId

    interface ISecrets with
        member _.BotTokenEncryptionKey = configuration.GetValue<string> "BotTokenEncryptionKey"
