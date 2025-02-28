namespace Lattice.Web

open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
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

    // ----- Auth -----

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

    // ----- Application -----

    let registerApplication discordBotToken (client: Api) = task {
        let payload = { DiscordBotToken = discordBotToken }
        let content = HttpContent.json RegisterApplicationPayload.encoder payload

        let! res = client.PostAsync("/applications", content)
        return! HttpResponseMessage.decode ApplicationResponse.decoder res
    }

    let getApplication (applicationId: string) (client: Api) = task {
        let! res = client.GetAsync($"/applications/{applicationId}")
        return! HttpResponseMessage.decode ApplicationResponse.decoder res
    }

    let updateApplication (applicationId: string) discordBotToken intents shardCount handler (client: Api) = task {
        let payload = { DiscordBotToken = discordBotToken; Intents = intents; ShardCount = shardCount; Handler = handler }
        let content = HttpContent.json UpdateApplicationPayload.encoder payload

        let! res = client.PatchAsync($"/applications/{applicationId}", content)
        return! HttpResponseMessage.decode ApplicationResponse.decoder res
    }

    let deleteApplication (applicationId: string) (client: Api) = task {
        let! res = client.DeleteAsync($"/applications/{applicationId}")
        return! HttpResponseMessage.unit res
    }

    let syncApplicationPrivilegedIntents (applicationId: string) (client: Api) = task {
        let! res = client.PostAsync($"/applications/{applicationId}/sync-privileged-intents", null)
        return! HttpResponseMessage.decode PrivilegedIntentsResponse.decoder res
    }

    // ----- Disabled Reasons -----

    let addDisabledApplicationReason (applicationId: string) (disabledReason: DisabledApplicationReason) (client: Api) = task {
        let! res= client.PutAsync($"/applications/{applicationId}/disabled-reasons/{int disabledReason}", null)
        return! HttpResponseMessage.decode DisabledApplicationReasonResponse.decoder res
    }

    let removeDisabledApplicationReason (applicationId: string) (disabledReason: DisabledApplicationReason) (client: Api) = task {
        let! res= client.DeleteAsync($"/applications/{applicationId}/disabled-reasons/{int disabledReason}")
        return! HttpResponseMessage.decode DisabledApplicationReasonResponse.decoder res
    }
