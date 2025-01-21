module Lattice.Orchestrator.Infrastructure.Discord.Discord

open FSharp.Discord.Rest

let getApplicationInformation (env: #IDiscordClientFactory) token = task {
    match! env.CreateBotClient token |> Rest.getCurrentApplication with
    | Ok { Data = app } -> return app |> ApplicationModel.toDomain |> Some
    | Error _ -> return None
}
