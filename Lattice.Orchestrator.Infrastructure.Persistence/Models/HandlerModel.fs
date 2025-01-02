namespace Lattice.Orchestrator.Infrastructure.Persistence

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
