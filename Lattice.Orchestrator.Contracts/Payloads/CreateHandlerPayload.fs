namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type CreateWebhookHandlerPayload = {
    Endpoint: string
}

module CreateWebhookHandlerPayload =
    let decoder: Decoder<CreateWebhookHandlerPayload> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field "endpoint" Decode.string
        })

    let encoder (v: CreateWebhookHandlerPayload) =
        Encode.object [
            "endpoint", Encode.string v.Endpoint
        ]

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

    let encoder (v: CreateServiceBusHandlerPayload) =
        Encode.object [
            "connectionString", Encode.string v.ConnectionString
            "queueName", Encode.string v.QueueName
        ]

type CreateHandlerPayload =
    | WEBHOOK of CreateWebhookHandlerPayload
    | SERVICE_BUS of CreateServiceBusHandlerPayload

module CreateHandlerPayload =
    let decoder: Decoder<CreateHandlerPayload> =
        Decode.oneOf [
            Decode.map WEBHOOK CreateWebhookHandlerPayload.decoder
            Decode.map SERVICE_BUS CreateServiceBusHandlerPayload.decoder
        ]

    let encoder (v: CreateHandlerPayload) =
        match v with
        | WEBHOOK v -> CreateWebhookHandlerPayload.encoder v
        | SERVICE_BUS v -> CreateServiceBusHandlerPayload.encoder v
