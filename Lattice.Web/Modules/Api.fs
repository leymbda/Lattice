namespace Lattice.Web

open Lattice.Orchestrator.Contracts
open System
open System.Net.Http
open System.Text
open Thoth.Json.Net

[<RequireQualifiedAccess>]
type ApiError =
    | Api of ErrorResponse
    | Serialization of string

module HttpContent =
    let json (encoder: Encoder<'a>) (payload: 'a) =
        let json = Encode.toString 4 (encoder payload)
        new StringContent(json, Encoding.UTF8, "application/json")

module HttpResponseMessage =
    let decode (decoder: Decoder<'a>) (res: HttpResponseMessage) = task {
        let! body = res.Content.ReadAsStringAsync()

        match res.IsSuccessStatusCode with
        | false ->
            return
                Decode.fromString ErrorResponse.decoder body
                |> function | Error e -> ApiError.Serialization e | Ok e -> ApiError.Api e
                |> Error

        | true ->
            return
                Decode.fromString decoder body
                |> Result.mapError (fun e -> ApiError.Serialization e)
    }

    let unit (res: HttpResponseMessage) = task {
        let! body = res.Content.ReadAsStringAsync()

        match res.IsSuccessStatusCode with
        | false ->
            return
                Decode.fromString ErrorResponse.decoder body
                |> function | Error e -> ApiError.Serialization e | Ok e -> ApiError.Api e
                |> Error

        | true ->
            return Ok ()  
    }

type Api = HttpClient

module Api =
    let create (baseUrl: string): Api =
        use httpClient = new HttpClient()
        httpClient.BaseAddress <- Uri baseUrl
        httpClient

    let login code redirectUri (client: Api) = task {
        let payload = { Code = code; RedirectUri = redirectUri }
        let content = HttpContent.json LoginPayload.encoder payload

        let! res = client.PostAsync("/auth/login", content)
        return! HttpResponseMessage.decode UserResponse.decoder res
    }

    let logout (client: Api) = task {
        let! res = client.PostAsync("/auth/logout", null)
        return! HttpResponseMessage.unit res
    }
