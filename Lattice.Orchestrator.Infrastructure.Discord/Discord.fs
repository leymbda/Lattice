module Lattice.Orchestrator.Infrastructure.Discord.Discord

open FSharp.Discord.Rest

let getApplicationInformation (discordClientFactory: IDiscordClientFactory) botToken = task {
    match! discordClientFactory.CreateBotClient botToken |> Rest.getCurrentApplication with
    | Error _ -> return None
    | Ok { Data = app } -> return app |> ApplicationModel.toDomain |> Some
}

let getUserInformation (discordClientFactory: IDiscordClientFactory) accessToken = task {
    match! discordClientFactory.CreateOAuthClient accessToken |> Rest.getCurrentUser with
    | Error _ -> return None
    | Ok { Data = user } -> return user |> UserModel.toDomain |> Some
}
