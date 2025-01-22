namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open System.Text.Json.Serialization

type WebhookHandlerResponse (endpoint, ed25519PublicKey) =
    [<JsonPropertyName "endpoint">]
    member _.Endpoint: string = endpoint

    [<JsonPropertyName "ed25519PublicKey">]
    member _.Ed25519PublicKey: string = ed25519PublicKey

module WebhookHandlerResponse =
    let fromDomain (handler: WebhookHandler) =
        WebhookHandlerResponse(handler.Endpoint, handler.Ed25519PublicKey)
        
type ServiceBusHandlerResponse (queueName) =
    [<JsonPropertyName "queueName">]
    member _.QueueName: string = queueName

module ServiceBusHandlerResponse =
    let fromDomain (handler: ServiceBusHandler) =
        ServiceBusHandlerResponse(handler.QueueName)

// The `HandlerResponse` must contain all possible properties from webhook and service bus handlers. It is notably used
// in the `ApplicationResponse` where a handler could be of any type.

[<AllowNullLiteral>]
type HandlerResponse (endpoint, ed25519PublicKey, queueName) =
    [<JsonPropertyName "endpoint">]
    [<OpenApiProperty(Nullable = true)>]
    member _.Endpoint: string = endpoint

    [<JsonPropertyName "ed25519PublicKey">]
    [<OpenApiProperty(Nullable = true)>]
    member _.Ed25519PublicKey: string = ed25519PublicKey
    
    [<JsonPropertyName "queueName">]
    [<OpenApiProperty(Nullable = true)>]
    member _.QueueName: string = queueName
    
module HandlerResponse =
    let fromDomain (handler: Handler option) =
        match handler with
        | Some (Handler.WEBHOOK handler) -> HandlerResponse(handler.Endpoint, handler.Ed25519PublicKey, null)
        | Some (Handler.SERVICE_BUS handler) -> HandlerResponse(null, null, handler.QueueName)
        | None -> null
