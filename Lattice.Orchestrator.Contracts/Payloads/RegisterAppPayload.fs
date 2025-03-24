namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type RegisterAppPayload = {
    DiscordBotToken: string
}

module RegisterAppPayload =
    let decoder: Decoder<RegisterAppPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Required.Field "discordBotToken" Decode.string
        })

    let encoder (v: RegisterAppPayload) =
        Encode.object [
            "discordBotToken", Encode.string v.DiscordBotToken
        ]
