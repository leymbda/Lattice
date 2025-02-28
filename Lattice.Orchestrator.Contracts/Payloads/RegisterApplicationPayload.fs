namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type RegisterApplicationPayload = {
    DiscordBotToken: string
}

module RegisterApplicationPayload =
    let decoder: Decoder<RegisterApplicationPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Required.Field "discordBotToken" Decode.string
        })

    let encoder (v: RegisterApplicationPayload) =
        Encode.object [
            "discordBotToken", Encode.string v.DiscordBotToken
        ]
