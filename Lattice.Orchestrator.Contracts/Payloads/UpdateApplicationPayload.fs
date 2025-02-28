namespace Lattice.Orchestrator.Contracts

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

    let encoder (v: UpdateApplicationPayload) =
        Encode.object [
            match v.DiscordBotToken with | Some p -> "discordBotToken", Encode.string p | None -> ()
            match v.Intents with | Some p -> "intents", Encode.int p | None -> ()
            match v.ShardCount with | Some p -> "shardCount", Encode.int p | None -> ()
            match v.Handler with | Some p -> "handler", Encode.option CreateHandlerPayload.encoder p | None -> ()
        ]
