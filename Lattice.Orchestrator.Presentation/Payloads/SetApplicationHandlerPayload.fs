namespace Lattice.Orchestrator.Presentation

open System.Text.Json
open System.Text.Json.Serialization

type SetApplicationHandlerPayloadType =
    | WEBHOOK     = 0
    | SERVICE_BUS = 1

type SetWebhookApplicationHandlerPayload = {
    [<JsonPropertyName "type">] Type: SetApplicationHandlerPayloadType
    [<JsonPropertyName "endpoint">] Endpoint: string
    [<JsonPropertyName "ed25519PublicKey">] Ed25519PublicKey: string
}

type SetServiceBusApplicationHandlerPayload = {
    [<JsonPropertyName "type">] Type: SetApplicationHandlerPayloadType
    [<JsonPropertyName "queueName">] QueueName: string
}

[<JsonConverter(typeof<SetApplicationHandlerPayloadConverter>)>]
type SetApplicationHandlerPayload =
    | WEBHOOK     of SetWebhookApplicationHandlerPayload
    | SERVICE_BUS of SetServiceBusApplicationHandlerPayload

and SetApplicationHandlerPayloadConverter () =
    inherit JsonConverter<SetApplicationHandlerPayload>()

    override _.Read (reader, _, _) =
        let success, document = JsonDocument.TryParseValue(&reader)
        if not success then raise (JsonException "Invalid SetApplicationHandlerPayloadType provided")

        let handlerType = document.RootElement.GetProperty "type" |> _.GetInt32() |> enum<SetApplicationHandlerPayloadType>
        let json = document.RootElement.GetRawText()

        match handlerType with
        | SetApplicationHandlerPayloadType.WEBHOOK -> SetApplicationHandlerPayload.WEBHOOK <| JsonSerializer.Deserialize<SetWebhookApplicationHandlerPayload> json
        | SetApplicationHandlerPayloadType.SERVICE_BUS -> SetApplicationHandlerPayload.SERVICE_BUS <| JsonSerializer.Deserialize<SetServiceBusApplicationHandlerPayload> json
        | _ -> raise (JsonException "Invalid SetApplicationHandlerPayloadType provided")

    override _.Write (writer, value, _) =
        let json =
            match value with
            | SetApplicationHandlerPayload.WEBHOOK payload -> JsonSerializer.Serialize payload
            | SetApplicationHandlerPayload.SERVICE_BUS payload -> JsonSerializer.Serialize payload

        writer.WriteRawValue json
