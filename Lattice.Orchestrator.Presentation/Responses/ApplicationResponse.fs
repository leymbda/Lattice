namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type ApplicationResponse = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
    [<JsonPropertyName "privilegedIntents">] PrivilegedIntents: PrivilegedIntentsResponse
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int
    [<JsonPropertyName "intents">] Intents: int
    [<JsonPropertyName "shardCount">] ProvisionedShardCount: int
    [<JsonPropertyName "handler">] Handler: HandlerResponse option
}

module ApplicationResponse =
    let fromDomain (application: Application) =
        {
            Id = application.Id
            DiscordBotToken = application.DiscordBotToken
            PrivilegedIntents = PrivilegedIntentsResponse.fromDomain application.PrivilegedIntents
            DisabledReasons = DisabledApplicationReason.toBitfield application.DisabledReasons
            Intents = application.Intents
            ProvisionedShardCount = application.ProvisionedShardCount
            Handler = Option.map HandlerResponse.fromDomain application.Handler
        }
