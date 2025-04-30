namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type RegisterAppPayload = {
    DiscordBotToken: string
}

module RegisterAppPayload =
    module Property =
        let [<Literal>] DiscordBotToken = "discordBotToken"

    let decoder: Decoder<RegisterAppPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Required.Field Property.DiscordBotToken Decode.string
        })

    let encoder (v: RegisterAppPayload) =
        Encode.object [
            Property.DiscordBotToken, Encode.string v.DiscordBotToken
        ]
