module Lattice.Orchestrator.Infrastructure.Discord.DiscordApi

open FSharp.Discord.Rest
open FSharp.Discord.Rest.Modules
open Lattice.Orchestrator.Application
open System.Net.Http

let getApplicationInformation (httpClientFactory: IHttpClientFactory): GetApplicationInformation = fun token -> task {
    let client = httpClientFactory.CreateClient() |> HttpClient.toBotClient token

    match! client |> Rest.getCurrentApplication with
    | Ok { Data = app } -> return app |> DiscordApplicationMapper.toDomain |> Ok
    | Error _ -> return Error ()
}
