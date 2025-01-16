module Lattice.Orchestrator.Infrastructure.Discord.DiscordApi

open FSharp.Discord.Rest
open Lattice.Orchestrator.Application

let getApplicationInformation (env: #IDiscordApiClientFactory): GetApplicationInformation = fun token -> task {
    match! env.BotClient token |> Rest.getCurrentApplication with
    | Ok { Data = app } -> return app |> DiscordApplicationMapper.toDomain |> Ok
    | Error _ -> return Error ()
}
