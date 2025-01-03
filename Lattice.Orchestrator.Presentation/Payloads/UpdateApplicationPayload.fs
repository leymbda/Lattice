namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type UpdateApplicationPayload = {
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string option
    [<JsonPropertyName "intents">] Intents: int option
    [<JsonPropertyName "shardCount">] ShardCount: int option
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int option
}
