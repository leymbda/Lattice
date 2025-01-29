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
    inherit IPersistence
    inherit IEvents

type Env (
    discordClientFactory: IDiscordClientFactory,
    cosmosClient: CosmosClient,
    eventGridPublisherClient: EventGridPublisherClient,
    serviceBusClient: ServiceBusClient
) =
    interface IEnv

    interface IDiscord with
        member _.GetApplicationInformation token = Discord.getApplicationInformation discordClientFactory token

    interface IEvents with
        member _.NodeHeartbeat nodeId heartbeatTime = EventGrid.nodeHeartbeat eventGridPublisherClient nodeId heartbeatTime
        member _.NodeRelease nodeId = ServiceBus.nodeRelease serviceBusClient nodeId
        member _.NodeRedistribute nodeId = ServiceBus.nodeRedistribute serviceBusClient nodeId
    
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
        