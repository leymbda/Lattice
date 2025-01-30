namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System
open System.Text.Json.Serialization

type UserModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "accessToken">] AccessToken: string
    [<JsonPropertyName "refreshToken">] RefreshToken: string
}

module UserModel =
    let toDomain (model: UserModel): User = {
        Id = model.Id
        AccessToken = model.AccessToken
        RefreshToken = model.RefreshToken
    }

    let fromDomain (user: User): UserModel = {
        Id = user.Id
        AccessToken = user.AccessToken
        RefreshToken = user.RefreshToken
    }
    