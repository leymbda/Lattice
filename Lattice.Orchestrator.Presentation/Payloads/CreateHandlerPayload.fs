namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type CreateWebhookHandlerPayload = {
    Endpoint: string
}

module CreateWebhookHandlerPayload =
    let decoder: Decoder<CreateWebhookHandlerPayload> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field "endpoint" Decode.string
        })

type CreateServiceBusHandlerPayload = {
    ConnectionString: string
    QueueName: string
}

module CreateServiceBusHandlerPayload =
    let decoder: Decoder<CreateServiceBusHandlerPayload> =
        Decode.object (fun get -> {
            ConnectionString = get.Required.Field "connectionString" Decode.string
            QueueName = get.Required.Field "queueName" Decode.string
        })

type CreateHandlerPayload =
    | WEBHOOK of CreateWebhookHandlerPayload
    | SERVICE_BUS of CreateServiceBusHandlerPayload

module CreateHandlerPayload =
    let decoder: Decoder<CreateHandlerPayload> =
        Decode.oneOf [
            Decode.map WEBHOOK CreateWebhookHandlerPayload.decoder
            Decode.map SERVICE_BUS CreateServiceBusHandlerPayload.decoder
        ]
