namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Rest
open System.Net.Http

type DiscordClientFactory (httpClientFactory: IHttpClientFactory) =
    interface IDiscordClientFactory with
        member _.CreateBotClient token = httpClientFactory.CreateClient() |> HttpClient.toBotClient token
        member _.CreateOAuthClient token = httpClientFactory.CreateClient() |> HttpClient.toOAuthClient token
        member _.CreateBasicClient clientId clientSecret = httpClientFactory.CreateClient() |> HttpClient.toBasicClient clientId clientSecret
