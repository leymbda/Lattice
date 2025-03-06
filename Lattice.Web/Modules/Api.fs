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
    // ----- Auth -----

    let login code redirectUri =
        let payload = {
            Code = code
            RedirectUri = redirectUri
        }

        Http.create POST "/api/auth/login"
        |> Http.encode LoginPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode UserResponse.decoder)

    let logout () =
        Http.create POST "/api/auth/logout"
        |> Http.send
        |> Async.map Http.unit

    // ----- Application -----

    let registerApplication discordBotToken =
        let payload = {
            DiscordBotToken = discordBotToken
        }

        Http.create POST "/api/applications"
        |> Http.encode RegisterApplicationPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode ApplicationResponse.decoder)

    let getApplication (applicationId: string) =
        Http.create GET $"/api/applications/{applicationId}"
        |> Http.send
        |> Async.map (Http.decode ApplicationResponse.decoder)

    let updateApplication (applicationId: string) discordBotToken intents shardCount handler =
        let payload = {
            DiscordBotToken = discordBotToken
            Intents = intents
            ShardCount = shardCount
            Handler = handler
        }

        Http.create PATCH $"/api/applications/{applicationId}"
        |> Http.encode UpdateApplicationPayload.encoder payload
        |> Http.send
        |> Async.map (Http.decode ApplicationResponse.decoder)

    let deleteApplication (applicationId: string) =
        Http.create DELETE $"/api/applications/{applicationId}"
        |> Http.send
        |> Async.map Http.unit

    let syncApplicationPrivilegedIntents (applicationId: string) =
        Http.create POST $"/api/applications/{applicationId}/sync-privileged-intents"
        |> Http.send
        |> Async.map (Http.decode PrivilegedIntentsResponse.decoder)

    // ----- Disabled Reasons -----

    let addDisabledApplicationReason (applicationId: string) (disabledReason: DisabledApplicationReason) =
        Http.create PUT $"/api/applications/{applicationId}/disabled-reasons/{int disabledReason}"
        |> Http.send
        |> Async.map (Http.decode DisabledApplicationReasonResponse.decoder)

    let removeDisabledApplicationReason (applicationId: string) (disabledReason: DisabledApplicationReason) =
        Http.create DELETE $"/api/applications/{applicationId}/disabled-reasons/{int disabledReason}"
        |> Http.send
        |> Async.map (Http.decode DisabledApplicationReasonResponse.decoder)
