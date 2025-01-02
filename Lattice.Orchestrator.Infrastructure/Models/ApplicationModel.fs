namespace Lattice.Orchestrator.Infrastructure

open System.Text.Json
open System.Text.Json.Serialization

type ApplicationModelType =
    | REGISTERED = 0
    | ACTIVATED  = 1

type RegisteredApplicationModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "type">] Type: ApplicationModelType
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
}

type ActivatedApplicationModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "type">] Type: ApplicationModelType
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
    [<JsonPropertyName "intents">] Intents: int
    [<JsonPropertyName "provisionedShardCount">] ProvisionedShardCount: int
    [<JsonPropertyName "handler">] Handler: HandlerModel option
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int
}

[<JsonConverter(typeof<ApplicationModelConverter>)>]
type ApplicationModel =
    | REGISTERED  of RegisteredApplicationModel
    | ACTIVATED   of ActivatedApplicationModel
    
and ApplicationModelConverter () =
    inherit JsonConverter<ApplicationModel>()

    override _.Read (reader, _, _) =
        let success, document = JsonDocument.TryParseValue(&reader)
        if not success then raise (JsonException "Invalid ApplicationModel provided")

        let applicationType = document.RootElement.GetProperty "type" |> _.GetInt32() |> enum<ApplicationModelType>
        let json = document.RootElement.GetRawText()

        match applicationType with
        | ApplicationModelType.REGISTERED -> ApplicationModel.REGISTERED <| JsonSerializer.Deserialize<RegisteredApplicationModel> json
        | ApplicationModelType.ACTIVATED -> ApplicationModel.ACTIVATED <| JsonSerializer.Deserialize<ActivatedApplicationModel> json
        | _ -> raise (JsonException "Invalid ApplicationModel provided")

    override _.Write (writer, value, _) =
        let json =
            match value with
            | ApplicationModel.REGISTERED model -> JsonSerializer.Serialize model
            | ApplicationModel.ACTIVATED model -> JsonSerializer.Serialize model

        writer.WriteRawValue json
