module Lattice.Orchestrator.Infrastructure.Discord.Discord

open FSharp.Discord.Rest

let getApplicationInformation (discordClientFactory: IDiscordClientFactory) botToken = task {
    let! res = discordClientFactory.CreateBotClient botToken |> Rest.getCurrentApplication
    return res |> Result.toOption |> Option.map _.Data
}

let getUserInformation (discordClientFactory: IDiscordClientFactory) accessToken = task {
    let! res = discordClientFactory.CreateOAuthClient accessToken |> Rest.getCurrentUser
    return res |> Result.toOption |> Option.map _.Data
}
