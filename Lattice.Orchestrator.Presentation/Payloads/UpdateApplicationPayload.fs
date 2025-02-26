namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type UpdateApplicationPayload = {
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    DisabledReasons: int option
}

module UpdateApplicationPayload =
    let decoder: Decoder<UpdateApplicationPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Optional.Field "discordBotToken" Decode.string
            Intents = get.Optional.Field "intents" Decode.int
            ShardCount = get.Optional.Field "shardCount" Decode.int
            DisabledReasons = get.Optional.Field "disabledReasons" Decode.int
        })
