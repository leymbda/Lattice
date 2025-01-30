namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type UserModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "accessToken">] AccessToken: string
    [<JsonPropertyName "refreshToken">] RefreshToken: string
}

module UserModel =
    let toDomain (model: UserModel): User = {
        Id = model.Id
        EncryptedAccessToken = model.AccessToken
        EncryptedRefreshToken = model.RefreshToken
    }

    let fromDomain (user: User): UserModel = {
        Id = user.Id
        AccessToken = user.EncryptedAccessToken
        RefreshToken = user.EncryptedRefreshToken
    }
    