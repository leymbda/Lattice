namespace Lattice.Web

open Fable.SimpleHttp
open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
open Thoth.Json.Net

[<RequireQualifiedAccess>]
type ApiError =
    | Api of ErrorResponse
    | Serialization of string

module Http =
    let create (method: HttpMethod) (url: string) =
        Http.request url |> Http.method method

    let encode (encoder: Encoder<'a>) (payload: 'a) (req: HttpRequest) =
        let json = Encode.toString 4 (encoder payload)
        
        req
        |> Http.header (Headers.contentType "application/json")
        |> Http.content (BodyContent.Text json)

    let decode (decoder: Decoder<'a>) (res: HttpResponse) =
        match res.statusCode, res.content with
        | v, ResponseContent.Text data when v >= 200 && v < 300 ->
            Decode.fromString decoder data
            |> Result.mapError (fun e -> ApiError.Serialization e)

        | _, ResponseContent.Text data ->
            Decode.fromString ErrorResponse.decoder data
            |> function | Error e -> ApiError.Serialization e | Ok e -> ApiError.Api e
            |> Error

        | _, _ ->
            Error (ApiError.Serialization "Invalid response content")

    let unit (res: HttpResponse) =
        match res.statusCode, res.content with
        | v, _ when v >= 200 && v < 300 -> Ok ()

        | _, ResponseContent.Text data ->
            Decode.fromString ErrorResponse.decoder data
            |> function | Error e -> ApiError.Serialization e | Ok e -> ApiError.Api e
            |> Error

        | _, _ ->
            Error (ApiError.Serialization "Invalid response content")

module Async =
    let map f a = async {
        let! r = a
        return f r
    }

module Api =
    let registerApplication discordBotToken =
        let payload = {
            DiscordBotToken = discordBotToken
        }

        Http.create POST "/api/applications"
        |> Http.encode RegisterAppPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode AppResponse.decoder)

    let getApplication (appId: string) =
        Http.create GET $"/api/applications/{appId}"
        |> Http.send
        |> Async.map (Http.decode AppResponse.decoder)

    let updateApplication (appId: string) discordBotToken intents shardCount handler =
        let payload = {
            DiscordBotToken = discordBotToken
            Intents = intents
            ShardCount = shardCount
            Handler = handler
        }

        Http.create PATCH $"/api/applications/{appId}"
        |> Http.encode UpdateAppPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode AppResponse.decoder)

    let deleteApplication (appId: string) =
        Http.create DELETE $"/api/applications/{appId}"
        |> Http.send
        |> Async.map Http.unit

    let syncApplicationPrivilegedIntents (appId: string) =
        Http.create POST $"/api/applications/{appId}/sync-privileged-intents"
        |> Http.send
        |> Async.map (Http.decode PrivilegedIntentsResponse.decoder)

    let addDisabledApplicationReason (appId: string) (disabledReason: DisabledApplicationReason) =
        Http.create PUT $"/api/applications/{appId}/disabled-reasons/{int disabledReason}"
        |> Http.send
        |> Async.map (Http.decode DisabledApplicationReasonResponse.decoder)

    let removeDisabledApplicationReason (appId: string) (disabledReason: DisabledApplicationReason) =
        Http.create DELETE $"/api/applications/{appId}/disabled-reasons/{int disabledReason}"
        |> Http.send
        |> Async.map (Http.decode DisabledApplicationReasonResponse.decoder)
