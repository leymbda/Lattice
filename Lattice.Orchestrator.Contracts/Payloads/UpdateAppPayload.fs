namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type UpdateAppPayload = {
    DiscordBotToken: string option
    Intents: int option
    ShardCount: int option
    Handler: CreateHandlerPayload option option
}

module UpdateAppPayload =
    module Property =
        let [<Literal>] DiscordBotToken = "discordBotToken"
        let [<Literal>] Intents = "intents"
        let [<Literal>] ShardCount = "shardCount"
        let [<Literal>] Handler = "handler"

    let decoder: Decoder<UpdateAppPayload> =
        Decode.object (fun get -> {
            DiscordBotToken = get.Optional.Field Property.DiscordBotToken Decode.string
            Intents = get.Optional.Field Property.Intents Decode.int
            ShardCount = get.Optional.Field Property.ShardCount Decode.int
            Handler = get.Optional.Raw (Decode.field Property.Handler (Decode.option CreateHandlerPayload.decoder))
        })

    let encoder (v: UpdateAppPayload) =
        Encode.object [
            match v.DiscordBotToken with | Some p -> Property.DiscordBotToken, Encode.string p | None -> ()
            match v.Intents with | Some p -> Property.Intents, Encode.int p | None -> ()
            match v.ShardCount with | Some p -> Property.ShardCount, Encode.int p | None -> ()
            match v.Handler with | Some p -> Property.Handler, Encode.option CreateHandlerPayload.encoder p | None -> ()
        ]
