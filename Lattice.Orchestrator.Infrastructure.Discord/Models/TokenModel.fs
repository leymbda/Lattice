namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Rest
open FSharp.Discord.Types
open Lattice.Orchestrator.Application

type TokenModel = AuthorizationCodeGrantResponse

module TokenModel =
    let toDomain (app: TokenModel): IDiscordToken =
        { new IDiscordToken with
            member _.AccessToken = app.AccessToken
            member _.TokenType = app.TokenType
            member _.ExpiresIn = app.ExpiresIn
            member _.RefreshToken = app.RefreshToken
            member _.Scope = app.Scope |> List.map OAuth2Scope.toString }
