module Lattice.Orchestrator.Presentation.HttpResponseData

open Microsoft.Azure.Functions.Worker.Http
open Thoth.Json.Net

let withHeader (key: string) (value: string) (res: HttpResponseData) =
    res.Headers.Remove(key) |> ignore
    res.Headers.Add(key, value)
    res

let withCookie (cookie: IHttpCookie) (res: HttpResponseData) =
    res.Cookies.Append(cookie)
    res

let withResponse (encoder: Encoder<'a>) (value: 'a) (res: HttpResponseData) = task {
    do! res.WriteStringAsync (Encode.toString 4 (encoder value))
    return res |> withHeader "Content-Type" "application/json"
}

let withErrorResponse (error: ErrorResponse) (res: HttpResponseData) =
    res |> withResponse ErrorResponse.encoder error
