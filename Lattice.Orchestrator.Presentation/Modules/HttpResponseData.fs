module Lattice.Orchestrator.Presentation.HttpResponseData

open Microsoft.Azure.Functions.Worker.Http
open Thoth.Json.Net

let withResponse (encoder: Encoder<'a>) (value: 'a) (res: HttpResponseData) = task {
    do! res.WriteStringAsync (Encode.toString 4 (encoder value))
    res.Headers.Remove("Content-Type") |> ignore
    res.Headers.Add("Content-Type", "application/json")
    return res
}

let withErrorResponse (error: ErrorResponse) (res: HttpResponseData) =
    res |> withResponse ErrorResponse.encoder error
