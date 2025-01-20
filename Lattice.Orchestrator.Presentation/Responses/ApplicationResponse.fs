namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json
open System.Text.Json.Serialization

type ApplicationResponseType =
    | REGISTERED = 0
    | ACTIVATED  = 1

type RegisteredApplicationResponse = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "type">] Type: ApplicationResponseType
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
}

type ActivatedApplicationResponse = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "type">] Type: ApplicationResponseType
    [<JsonPropertyName "discordBotToken">] DiscordBotToken: string
    [<JsonPropertyName "intents">] Intents: int
    [<JsonPropertyName "shardCount">] ProvisionedShardCount: int
    [<JsonPropertyName "handler">] Handler: HandlerResponse option
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int
}

[<JsonConverter(typeof<ApplicationResponseConverter>)>]
type ApplicationResponse =
    | REGISTERED  of RegisteredApplicationResponse
    | ACTIVATED   of ActivatedApplicationResponse
    
and ApplicationResponseConverter () =
    inherit JsonConverter<ApplicationResponse>()

    override _.Read (reader, _, _) =
        let success, document = JsonDocument.TryParseValue(&reader)
        if not success then raise (JsonException "Invalid ApplicationResponse provided")

        let applicationType = document.RootElement.GetProperty "type" |> _.GetInt32() |> enum<ApplicationResponseType>
        let json = document.RootElement.GetRawText()

        match applicationType with
        | ApplicationResponseType.REGISTERED -> ApplicationResponse.REGISTERED <| JsonSerializer.Deserialize<RegisteredApplicationResponse> json
        | ApplicationResponseType.ACTIVATED -> ApplicationResponse.ACTIVATED <| JsonSerializer.Deserialize<ActivatedApplicationResponse> json
        | _ -> raise (JsonException "Invalid ApplicationResponse provided")

    override _.Write (writer, value, _) =
        let json =
            match value with
            | ApplicationResponse.REGISTERED response -> JsonSerializer.Serialize response
            | ApplicationResponse.ACTIVATED response -> JsonSerializer.Serialize response

        writer.WriteRawValue json

module ApplicationResponse =
    let fromDomain (application: Application) =
        match application with
        | Application.REGISTERED registeredApplication ->
            ApplicationResponse.REGISTERED {
                Id = registeredApplication.Id
                Type = ApplicationResponseType.REGISTERED
                DiscordBotToken = registeredApplication.DiscordBotToken
            }

        | Application.ACTIVATED activatedApplication ->
            ApplicationResponse.ACTIVATED {
                Id = activatedApplication.Id
                Type = ApplicationResponseType.ACTIVATED
                DiscordBotToken = activatedApplication.DiscordBotToken
                Intents = activatedApplication.Intents
                ProvisionedShardCount = activatedApplication.ProvisionedShardCount
                Handler = Option.map HandlerResponse.fromDomain activatedApplication.Handler
                DisabledReasons = activatedApplication.DisabledReasons
            }
