namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type LoginCommandProps = {
    Code: string
    RedirectUri: string
}

type LoginCommandError =
    | CodeExchangeFailed
    | LoginFailed

module LoginCommand =
    let run (env: #IDiscord & #IPersistence) (props: LoginCommandProps) = task {
        // Exchange code for token
        match! env.ExchangeCodeForAccessToken props.RedirectUri props.Code with
        | None -> return Error LoginCommandError.CodeExchangeFailed
        | Some token ->

        // Fetch user information from Discord
        match! env.GetUserInformation token.AccessToken with
        | None -> return Error LoginCommandError.LoginFailed
        | Some discordUser ->

        // Save user to db
        let user = User.create discordUser.Id token.AccessToken token.RefreshToken

        match! env.UpsertUser user with
        | Error _ -> return Error LoginCommandError.LoginFailed
        | Ok user ->

        // Generate JWT

        // TODO: Generate JWT and return it

        return Ok ""
    }
