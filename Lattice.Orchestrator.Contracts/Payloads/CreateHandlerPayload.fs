namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type CreateWebhookHandlerPayload = {
    Endpoint: string
}

module CreateWebhookHandlerPayload =
    module Property =
        let [<Literal>] Endpoint = "endpoint"

    let decoder: Decoder<CreateWebhookHandlerPayload> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field Property.Endpoint Decode.string
        })

    let encoder (v: CreateWebhookHandlerPayload) =
        Encode.object [
            Property.Endpoint, Encode.string v.Endpoint
        ]

type CreateServiceBusHandlerPayload = {
    ConnectionString: string
    QueueName: string
}

module CreateServiceBusHandlerPayload =
    module Property =
        let [<Literal>] ConnectionString = "connectionString"
        let [<Literal>] QueueName = "queueName"

    let decoder: Decoder<CreateServiceBusHandlerPayload> =
        Decode.object (fun get -> {
            ConnectionString = get.Required.Field Property.ConnectionString Decode.string
            QueueName = get.Required.Field Property.QueueName Decode.string
        })

    let encoder (v: CreateServiceBusHandlerPayload) =
        Encode.object [
            Property.ConnectionString, Encode.string v.ConnectionString
            Property.QueueName, Encode.string v.QueueName
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
