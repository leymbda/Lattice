module Lattice.Orchestrator.Infrastructure.Discord.Discord

open FSharp.Discord.Rest

let getApplicationInformation (env: #IDiscordApiClientFactory) token = task {
    match! env.CreateBotClient token |> Rest.getCurrentApplication with
    | Ok { Data = app } -> return app |> DiscordApplicationMapper.toDomain |> Some
    | Error _ -> return None
}
