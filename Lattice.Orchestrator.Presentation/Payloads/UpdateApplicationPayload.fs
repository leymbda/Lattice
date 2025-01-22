namespace Lattice.Orchestrator.Presentation

open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open System
open System.Text.Json.Serialization

type UpdateApplicationPayload (discordBotToken, intents, shardCount, disabledReasons) =
    [<JsonPropertyName "discordBotToken">]
    [<OpenApiProperty(Nullable = true)>]
    member _.DiscordBotToken: string = discordBotToken

    [<JsonPropertyName "intents">]
    member _.Intents: Nullable<int> = intents

    [<JsonPropertyName "shardCount">]
    member _.ShardCount: Nullable<int> = shardCount

    [<JsonPropertyName "disabledReasons">]
    member _.DisabledReasons: Nullable<int> = disabledReasons
