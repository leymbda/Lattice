namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type RegisterApplicationPayload (discordBotToken) =
    [<JsonPropertyName "discordBotToken">]
    member _.DiscordBotToken: string = discordBotToken
