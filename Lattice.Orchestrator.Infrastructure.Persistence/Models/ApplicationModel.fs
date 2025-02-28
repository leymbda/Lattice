namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type ApplicationModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
    [<JsonPropertyName "privilegedIntents">] PrivilegedIntents: PrivilegedIntentsModel
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int
    [<JsonPropertyName "intents">] Intents: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "handler">] Handler: HandlerModel option
}

module ApplicationModel =
    let toDomain (model: ApplicationModel): Application =
        {
            Id = model.Id
            EncryptedBotToken = model.DiscordBotToken
            PrivilegedIntents = PrivilegedIntentsModel.toDomain model.PrivilegedIntents
            DisabledReasons = DisabledApplicationReason.fromBitfield model.DisabledReasons
            Intents = model.Intents
            ShardCount = model.ShardCount
            Handler = Option.map HandlerModel.toDomain model.Handler
        }

    let fromDomain (application: Application): ApplicationModel =
        {
            Id = application.Id
            DiscordBotToken = application.EncryptedBotToken
            PrivilegedIntents = PrivilegedIntentsModel.fromDomain application.PrivilegedIntents
            DisabledReasons = DisabledApplicationReason.toBitfield application.DisabledReasons
            Intents = application.Intents
            ShardCount = application.ShardCount
            Handler = Option.map HandlerModel.fromDomain application.Handler
        }
