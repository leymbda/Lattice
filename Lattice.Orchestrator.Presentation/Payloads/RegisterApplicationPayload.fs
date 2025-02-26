namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type RegisterApplicationPayload = {
    DiscordBotToken: string
}

module RegisterApplicationPayload =
    let decoder: Decoder<RegisterApplicationPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Required.Field "discordBotToken" Decode.string
        })
