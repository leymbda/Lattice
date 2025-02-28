namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type UserResponse = {
    Id: string
    Username: string
}

module UserResponse =
    let decoder: Decoder<UserResponse> =
        Decode.object (fun get -> {
            Id = get.Required.Field "id" Decode.string
            Username = get.Required.Field "username" Decode.string
        })

    let encoder (v: UserResponse) =
        Encode.object [
            "id", Encode.string v.Id
            "username", Encode.string v.Username
        ]

    let fromDomain (v: User) = {
        Id = v.Id
        Username = v.Username
    }
