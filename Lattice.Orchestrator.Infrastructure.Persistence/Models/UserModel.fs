namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type UserModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "username">] Username: string
}

module UserModel =
    let toDomain (v: UserModel): User = {
        Id = v.Id
        Username = v.Username
    }

    let fromDomain (v: User): UserModel = {
        Id = v.Id
        Username = v.Username
    }
    