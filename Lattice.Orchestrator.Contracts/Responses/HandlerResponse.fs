namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type WebhookHandlerResponse = {
    Endpoint: string
    Ed25519PublicKey: string
}

module WebhookHandlerResponse =
    let decoder: Decoder<WebhookHandlerResponse> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field "endpoint" Decode.string
            Ed25519PublicKey = get.Required.Field "ed25519PublicKey" Decode.string
        })

    let encoder (v: WebhookHandlerResponse) =
        Encode.object [
            "endpoint", Encode.string v.Endpoint
            "ed25519PublicKey", Encode.string v.Ed25519PublicKey
        ]

    let fromDomain (handler: WebhookHandler) = {
        Endpoint = handler.Endpoint
        Ed25519PublicKey = handler.Ed25519PublicKey
    }
        
type ServiceBusHandlerResponse = {
    QueueName: string
}

module ServiceBusHandlerResponse =
    let decoder: Decoder<ServiceBusHandlerResponse> =
        Decode.object (fun get -> {
            QueueName = get.Required.Field "queueName" Decode.string
        })

    let encoder (v: ServiceBusHandlerResponse) =
        Encode.object [
            "queueName", Encode.string v.QueueName
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
