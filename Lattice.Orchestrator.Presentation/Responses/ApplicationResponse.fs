namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open System.Text.Json.Serialization

type ApplicationResponse (id, discordBotToken, privilegedIntents, disabledReasons, intents, provisionedShardCount, handler) =
    [<JsonPropertyName "id">]
    member _.Id: string = id

    [<JsonPropertyName "discordBotToken">]
    member _.DiscordBotToken: string = discordBotToken

    [<JsonPropertyName "privilegedIntents">]
    member _.PrivilegedIntents: PrivilegedIntentsResponse = privilegedIntents

    [<JsonPropertyName "disabledReasons">]
    member _.DisabledReasons: int = disabledReasons

    [<JsonPropertyName "intents">]
    member _.Intents: int = intents

    [<JsonPropertyName "shardCount">]
    member _.ProvisionedShardCount: int = provisionedShardCount

    [<JsonPropertyName "handler">]
    [<OpenApiProperty(Nullable = true)>]
    member _.Handler: HandlerResponse = handler

module ApplicationResponse =
    let fromDomain (application: Application) =
        ApplicationResponse(application.Id,
            application.DiscordBotToken,
            PrivilegedIntentsResponse.fromDomain application.PrivilegedIntents,
            DisabledApplicationReason.toBitfield application.DisabledReasons,
            application.Intents,
            application.ProvisionedShardCount,
            HandlerResponse.fromDomain application.Handler
        )
