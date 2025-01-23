module Lattice.Orchestrator.Infrastructure.Discord.Discord

open FSharp.Discord.Rest

let getApplicationInformation (discordClientFactory: IDiscordClientFactory) token = task {
    match! discordClientFactory.CreateBotClient token |> Rest.getCurrentApplication with
    | Ok { Data = app } -> return app |> ApplicationModel.toDomain |> Some
    | Error _ -> return None
}
