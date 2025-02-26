namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type SetWebhookApplicationHandlerPayload = {
    Endpoint: string
}

module SetWebhookApplicationHandlerPayload =
    let decoder: Decoder<SetWebhookApplicationHandlerPayload> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field "endpoint" Decode.string
        })
