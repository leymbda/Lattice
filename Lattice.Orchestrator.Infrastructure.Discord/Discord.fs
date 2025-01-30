module Lattice.Orchestrator.Infrastructure.Discord.Discord

open FSharp.Discord.Rest

let getApplicationInformation (discordClientFactory: IDiscordClientFactory) botToken = task {
    match! discordClientFactory.CreateBotClient botToken |> Rest.getCurrentApplication with
    | Error _ -> return None
    | Ok { Data = app } -> return app |> ApplicationModel.toDomain |> Some
}

let getUserInformation (discordClientFactory: IDiscordClientFactory) accessToken = task {
    let client = discordClientFactory.CreateOAuthClient accessToken
    match! client |> Rest.getCurrentUser with
    | Error _ -> return None
    | Ok { Data = user } -> return user |> UserModel.toDomain |> Some
}

let exchangeCodeForAccessToken (discordClientFactory: IDiscordClientFactory) clientId clientSecret redirectUri code = task {
    let client = discordClientFactory.CreateBasicClient clientId clientSecret
    let payload = AuthorizationCodeGrantPayload(code, redirectUri)

    match! client |> Rest.authorizationCodeGrant payload with
    | Error _ -> return None
    | Ok { Data = data } -> return data |> TokenModel.toDomain |> Some
}
