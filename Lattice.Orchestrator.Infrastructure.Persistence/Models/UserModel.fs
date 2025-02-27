namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type UserModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "username">] Username: string
    [<JsonPropertyName "accessToken">] AccessToken: string
    [<JsonPropertyName "refreshToken">] RefreshToken: string
}

module UserModel =
    let toDomain (v: UserModel): User = {
        Id = v.Id
        Username = v.Username
        EncryptedAccessToken = v.AccessToken
        EncryptedRefreshToken = v.RefreshToken
    }

    let fromDomain (v: User): UserModel = {
        Id = v.Id
        Username = v.Username
        AccessToken = v.EncryptedAccessToken
        RefreshToken = v.EncryptedRefreshToken
    }
    