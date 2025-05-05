namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type WebhookHandlerResponse = {
    Endpoint: string
    Ed25519PublicKey: string
}

module WebhookHandlerResponse =
    module Property =
        let [<Literal>] Endpoint = "endpoint"
        let [<Literal>] Ed25519PublicKey = "ed25519PublicKey"

    let decoder: Decoder<WebhookHandlerResponse> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field Property.Endpoint Decode.string
            Ed25519PublicKey = get.Required.Field Property.Ed25519PublicKey Decode.string
        })

    let encoder (v: WebhookHandlerResponse) =
        Encode.object [
            Property.Endpoint, Encode.string v.Endpoint
            Property.Ed25519PublicKey, Encode.string v.Ed25519PublicKey
        ]

    let fromDomain (handler: WebhookHandler) = {
        Endpoint = handler.Endpoint
        Ed25519PublicKey = handler.Ed25519PublicKey
    }
        
type ServiceBusHandlerResponse = {
    QueueName: string
}

module ServiceBusHandlerResponse =
    module Property =
        let [<Literal>] QueueName = "queueName"

    let decoder: Decoder<ServiceBusHandlerResponse> =
        Decode.object (fun get -> {
            QueueName = get.Required.Field Property.QueueName Decode.string
        })

    let encoder (v: ServiceBusHandlerResponse) =
        Encode.object [
            Property.QueueName, Encode.string v.QueueName
        ]

    let fromDomain (handler: ServiceBusHandler) = {
        QueueName = handler.QueueName
    }

type HandlerResponse =
    | WEBHOOK of WebhookHandlerResponse
    | SERVICE_BUS of ServiceBusHandlerResponse

module HandlerResponse =
    let decoder: Decoder<HandlerResponse> =
        Decode.oneOf [
            Decode.map WEBHOOK WebhookHandlerResponse.decoder
            Decode.map SERVICE_BUS ServiceBusHandlerResponse.decoder
        ]

    let encoder (v: HandlerResponse) =
        match v with
        | WEBHOOK handler -> WebhookHandlerResponse.encoder handler
        | SERVICE_BUS handler -> ServiceBusHandlerResponse.encoder handler

    let fromDomain (handler: Handler) =
        match handler with
        | Handler.WEBHOOK handler -> HandlerResponse.WEBHOOK (WebhookHandlerResponse.fromDomain handler)
        | Handler.SERVICE_BUS handler -> HandlerResponse.SERVICE_BUS (ServiceBusHandlerResponse.fromDomain handler)
