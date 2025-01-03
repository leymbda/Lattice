namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type RegisterApplicationPayload = {
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
}
