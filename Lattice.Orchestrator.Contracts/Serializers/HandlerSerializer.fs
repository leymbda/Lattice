namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

module WebhookHandler =
    module Property =
        let [<Literal>] Endpoint = "endpoint"
        let [<Literal>] Ed25519PublicKey = "ed25519PublicKey"
        let [<Literal>] Ed25519PrivateKey = "ed25519PrivateKey"

    let decoder: Decoder<WebhookHandler> =
        Decode.object (fun get -> {
            Endpoint = get.Required.Field Property.Endpoint Decode.string
            Ed25519PublicKey = get.Required.Field Property.Ed25519PublicKey Decode.string
            Ed25519PrivateKey = get.Required.Field Property.Ed25519PrivateKey Decode.string
        })

    let encoder (v: WebhookHandler) =
        Encode.object [
            Property.Endpoint, Encode.string v.Endpoint
            Property.Ed25519PublicKey, Encode.string v.Ed25519PublicKey
            Property.Ed25519PrivateKey, Encode.string v.Ed25519PrivateKey
        ]

module ServiceBusHandler =
    module Property =
        let [<Literal>] ConnectionString = "connectionString"
        let [<Literal>] QueueName = "queueName"

    let decoder: Decoder<ServiceBusHandler> =
        Decode.object (fun get -> {
            ConnectionString = get.Required.Field Property.ConnectionString Decode.string
            QueueName = get.Required.Field Property.QueueName Decode.string
        })

    let encoder (v: ServiceBusHandler) =
        Encode.object [
            Property.ConnectionString, Encode.string v.ConnectionString
            Property.QueueName, Encode.string v.QueueName
        ]

module Handler =
    let decoder: Decoder<Handler> =
        Decode.oneOf [
            Decode.map Handler.WEBHOOK WebhookHandler.decoder
            Decode.map Handler.SERVICE_BUS ServiceBusHandler.decoder
        ]

    let encoder (v: Handler) =
        match v with
        | Handler.WEBHOOK handler -> WebhookHandler.encoder handler
        | Handler.SERVICE_BUS handler -> ServiceBusHandler.encoder handler
