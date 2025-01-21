namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json
open System.Text.Json.Serialization

type HandlerModelType =
    | WEBHOOK     = 0
    | SERVICE_BUS = 1

type WebhookHandlerModel = {
    [<JsonPropertyName "type">] Type: HandlerModelType
    [<JsonPropertyName "endpoint">] Endpoint: string
    [<JsonPropertyName "ed25519PublicKey">] Ed25519PublicKey: string
    [<JsonPropertyName "ed25519PrivateKey">] Ed25519PrivateKey: string
}

type ServiceBusHandlerModel = {
    [<JsonPropertyName "type">] Type: HandlerModelType
    [<JsonPropertyName "connectionString">] ConnectionString: string
    [<JsonPropertyName "queueName">] QueueName: string
}

[<JsonConverter(typeof<HandlerModelConverter>)>]
type HandlerModel =
    | WEBHOOK     of WebhookHandlerModel
    | SERVICE_BUS of ServiceBusHandlerModel

and HandlerModelConverter () =
    inherit JsonConverter<HandlerModel>()

    override _.Read (reader, _, _) =
        let success, document = JsonDocument.TryParseValue(&reader)
        if not success then raise (JsonException "Invalid HandlerModel provided")

        let handlerType = document.RootElement.GetProperty "type" |> _.GetInt32() |> enum<HandlerModelType>
        let json = document.RootElement.GetRawText()

        match handlerType with
        | HandlerModelType.WEBHOOK -> HandlerModel.WEBHOOK <| JsonSerializer.Deserialize<WebhookHandlerModel> json
        | HandlerModelType.SERVICE_BUS -> HandlerModel.SERVICE_BUS <| JsonSerializer.Deserialize<ServiceBusHandlerModel> json
        | _ -> raise (JsonException "Invalid HandlerModel provided")

    override _.Write (writer, value, _) =
        let json =
            match value with
            | HandlerModel.WEBHOOK model -> JsonSerializer.Serialize model
            | HandlerModel.SERVICE_BUS model -> JsonSerializer.Serialize model

        writer.WriteRawValue json

module HandlerModel =
    let toDomain model =
        match model with
        | HandlerModel.WEBHOOK model ->
            Handler.WEBHOOK {
                Endpoint = model.Endpoint
                Ed25519PublicKey = model.Ed25519PublicKey
                Ed25519PrivateKey = model.Ed25519PrivateKey
            }

        | HandlerModel.SERVICE_BUS model ->
            Handler.SERVICE_BUS {
                ConnectionString = model.ConnectionString
                QueueName = model.QueueName
            }

    let fromDomain handler =
        match handler with
        | Handler.WEBHOOK handler ->
            HandlerModel.WEBHOOK {
                Type = HandlerModelType.WEBHOOK
                Endpoint = handler.Endpoint
                Ed25519PublicKey = handler.Ed25519PublicKey
                Ed25519PrivateKey = handler.Ed25519PrivateKey
            }

        | Handler.SERVICE_BUS handler ->
            HandlerModel.SERVICE_BUS {
                Type = HandlerModelType.SERVICE_BUS
                ConnectionString = handler.ConnectionString
                QueueName = handler.QueueName
            }
