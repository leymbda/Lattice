namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System

type LoginCommandProps = {
    Code: string
    RedirectUri: string
}

type LoginCommandError =
    | CodeExchangeFailed
    | LoginFailed

module LoginCommand =
    let run (env: #IDiscord & #IPersistence & #ISecrets) (props: LoginCommandProps) = task {
        // Exchange code for token
        match! env.ExchangeCodeForAccessToken props.RedirectUri props.Code with
        | None -> return Error LoginCommandError.CodeExchangeFailed
        | Some token ->

        // Fetch user information from Discord
        match! env.GetUserInformation token.AccessToken with
        | None -> return Error LoginCommandError.LoginFailed
        | Some discordUser ->

        // Save user to db
        let encryptedAccessToken = token.AccessToken |> Aes.encrypt env.UserAccessTokenEncryptionKey
        let encryptedRefreshToken = token.RefreshToken |> Aes.encrypt env.UserRefreshTokenEncryptionKey

        let user = User.create discordUser.Id discordUser.Username encryptedAccessToken encryptedRefreshToken

        match! env.UpsertUser user with
        | Error _ -> return Error LoginCommandError.LoginFailed
        | Ok user ->

        // Generate JWT
        let token =
            TokenClaims.create user.Id DateTime.UtcNow
            |> Jwt.create
            |> Jwt.encode env.JwtHashingKey

        return Ok (user, token)
    }
