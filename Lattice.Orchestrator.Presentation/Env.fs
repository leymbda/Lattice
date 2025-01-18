namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Infrastructure.Discord
open FSharp.Discord.Rest
open System.Net.Http

type IEnv =
    inherit IDiscordApiClientFactory
    inherit IDiscord

type Env (httpClientFactory: IHttpClientFactory) as env =
    interface IEnv

    interface IDiscordApiClientFactory with
        member _.CreateBotClient token = httpClientFactory.CreateClient() |> HttpClient.toBotClient token
        member _.CreateOAuthClient token = httpClientFactory.CreateClient() |> HttpClient.toOAuthClient token
        member _.CreateBasicClient clientId clientSecret = httpClientFactory.CreateClient() |> HttpClient.toBasicClient clientId clientSecret

    interface IDiscord with
        member _.GetApplicationInformation token = Discord.getApplicationInformation env token
