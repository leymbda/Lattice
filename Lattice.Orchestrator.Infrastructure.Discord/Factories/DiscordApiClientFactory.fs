namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Rest
open System.Net.Http

type DiscordApiClientFactory (httpClientFactory: IHttpClientFactory) = 
    interface IDiscordApiClientFactory with
        member _.BotClient token = httpClientFactory.CreateClient() |> HttpClient.toBotClient token
        member _.OAuthClient token = httpClientFactory.CreateClient() |> HttpClient.toOAuthClient token
        member _.BasicClient clientId clientSecret = httpClientFactory.CreateClient() |> HttpClient.toBasicClient clientId clientSecret
