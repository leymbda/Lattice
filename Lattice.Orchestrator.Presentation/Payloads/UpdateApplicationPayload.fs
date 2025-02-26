namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type UpdateApplicationPayload = {
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    Handler: CreateHandlerPayload option option
}

module UpdateApplicationPayload =
    let decoder: Decoder<UpdateApplicationPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Optional.Field "discordBotToken" Decode.string
            Intents = get.Optional.Field "intents" Decode.int
            ShardCount = get.Optional.Field "shardCount" Decode.int
            Handler = get.Optional.Raw (Decode.field "handler" (Decode.option CreateHandlerPayload.decoder))
        })
