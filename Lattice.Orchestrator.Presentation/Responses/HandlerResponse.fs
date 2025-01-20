namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json
open System.Text.Json.Serialization

type HandlerResponseType =
    | WEBHOOK     = 0
    | SERVICE_BUS = 1

type WebhookHandlerResponse = {
    [<JsonPropertyName "type">] Type: HandlerResponseType
    [<JsonPropertyName "endpoint">] Endpoint: string
    [<JsonPropertyName "ed25519PublicKey">] Ed25519PublicKey: string
}

type ServiceBusHandlerResponse = {
    [<JsonPropertyName "type">] Type: HandlerResponseType
    [<JsonPropertyName "queueName">] QueueName: string
}

[<JsonConverter(typeof<HandlerResponseConverter>)>]
type HandlerResponse =
    | WEBHOOK     of WebhookHandlerResponse
    | SERVICE_BUS of ServiceBusHandlerResponse

and HandlerResponseConverter () =
    inherit JsonConverter<HandlerResponse>()

    override _.Read (reader, _, _) =
        let success, document = JsonDocument.TryParseValue(&reader)
        if not success then raise (JsonException "Invalid HandlerResponse provided")

        let handlerType = document.RootElement.GetProperty "type" |> _.GetInt32() |> enum<HandlerResponseType>
        let json = document.RootElement.GetRawText()

        match handlerType with
        | HandlerResponseType.WEBHOOK -> HandlerResponse.WEBHOOK <| JsonSerializer.Deserialize<WebhookHandlerResponse> json
        | HandlerResponseType.SERVICE_BUS -> HandlerResponse.SERVICE_BUS <| JsonSerializer.Deserialize<ServiceBusHandlerResponse> json
        | _ -> raise (JsonException "Invalid HandlerResponse provided")

    override _.Write (writer, value, _) =
        let json =
            match value with
            | HandlerResponse.WEBHOOK response -> JsonSerializer.Serialize response
            | HandlerResponse.SERVICE_BUS response -> JsonSerializer.Serialize response

        writer.WriteRawValue json

module HandlerResponse =
    let fromDomain (handler: Handler) =
        match handler with
        | Handler.WEBHOOK webhookHandler ->
            HandlerResponse.WEBHOOK {
                Type = HandlerResponseType.WEBHOOK
                Endpoint = webhookHandler.Endpoint
                Ed25519PublicKey = webhookHandler.Ed25519PublicKey
            }

        | Handler.SERVICE_BUS serviceBusHandler ->
            HandlerResponse.SERVICE_BUS {
                Type = HandlerResponseType.SERVICE_BUS
                QueueName = serviceBusHandler.QueueName
            }
