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
    let create (method: HttpMethod) (parts: string list) =
        String.concat "/" parts
        |> Http.request
        |> Http.method method

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
    let registerApp discordBotToken =
        let payload = {
            DiscordBotToken = discordBotToken
        }

        Http.create POST ["api"; "apps"]
        |> Http.encode RegisterAppPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode AppResponse.decoder)

    let getApp (appId: string) =
        Http.create GET ["api"; "apps"; appId]
        |> Http.send
        |> Async.map (Http.decode AppResponse.decoder)

    let updateApp (appId: string) discordBotToken intents shardCount handler =
        let payload = {
            DiscordBotToken = discordBotToken
            Intents = intents
            ShardCount = shardCount
            Handler = handler
        }

        Http.create PATCH ["api"; "apps"; appId]
        |> Http.encode UpdateAppPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode AppResponse.decoder)

    let deleteApp (appId: string) =
        Http.create DELETE ["api"; "apps"; appId]
        |> Http.send
        |> Async.map Http.unit

    let syncAppPrivilegedIntents (appId: string) =
        Http.create POST ["api"; "apps"; appId; "sync-privileged-intents"]
        |> Http.send
        |> Async.map (Http.decode PrivilegedIntentsResponse.decoder)

    let addDisabledAppReason (appId: string) (disabledReason: DisabledAppReason) =
        Http.create PUT ["api"; "apps"; appId; "disabled-reasons"; disabledReason |> int |> string]
        |> Http.send
        |> Async.map (Http.decode DisabledAppReasonResponse.decoder)

    let removeDisabledAppReason (appId: string) (disabledReason: DisabledAppReason) =
        Http.create DELETE ["api"; "apps"; appId; "disabled-reasons"; disabledReason |> int |> string]
        |> Http.send
        |> Async.map (Http.decode DisabledAppReasonResponse.decoder)
