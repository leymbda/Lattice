namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type WebhookHandlerResponse = {
    Endpoint: string
    Ed25519PublicKey: string
}

module WebhookHandlerResponse =
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
    let encoder (v: ServiceBusHandlerResponse) =
        Encode.object [
            "queueName", Encode.string v.QueueName
        ]

    let fromDomain (handler: ServiceBusHandler) = {
        QueueName = handler.QueueName
    }

type HandlerResponse =
    | UNCONFIGURED
    | WEBHOOK of WebhookHandlerResponse
    | SERVICE_BUS of ServiceBusHandlerResponse

module HandlerResponse =
    let encoder (v: HandlerResponse) =
        match v with
        | WEBHOOK handler -> WebhookHandlerResponse.encoder handler
        | SERVICE_BUS handler -> ServiceBusHandlerResponse.encoder handler
        | UNCONFIGURED -> Encode.nil

    let fromDomain (handler: Handler option) =
        match handler with
        | Some (Handler.WEBHOOK handler) -> HandlerResponse.WEBHOOK (WebhookHandlerResponse.fromDomain handler)
        | Some (Handler.SERVICE_BUS handler) -> HandlerResponse.SERVICE_BUS (ServiceBusHandlerResponse.fromDomain handler)
        | None -> HandlerResponse.UNCONFIGURED
