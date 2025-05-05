namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type AppModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
    [<JsonPropertyName "privilegedIntents">] PrivilegedIntents: PrivilegedIntentsModel
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int
    [<JsonPropertyName "intents">] Intents: int
    [<JsonPropertyName "shardCount">] ShardCount: int
    [<JsonPropertyName "handler">] Handler: HandlerModel option
}

module AppModel =
    let toDomain (model: AppModel): App =
        {
            Id = model.Id
            EncryptedBotToken = model.DiscordBotToken
            PrivilegedIntents = PrivilegedIntentsModel.toDomain model.PrivilegedIntents
            DisabledReasons = DisabledAppReason.fromBitfield model.DisabledReasons
            Intents = model.Intents
            ShardCount = model.ShardCount
            Handler = Option.map HandlerModel.toDomain model.Handler
        }

    let fromDomain (app: App): AppModel =
        {
            Id = app.Id
            DiscordBotToken = app.EncryptedBotToken
            PrivilegedIntents = PrivilegedIntentsModel.fromDomain app.PrivilegedIntents
            DisabledReasons = DisabledAppReason.toBitfield app.DisabledReasons
            Intents = app.Intents
            ShardCount = app.ShardCount
            Handler = Option.map HandlerModel.fromDomain app.Handler
        }
