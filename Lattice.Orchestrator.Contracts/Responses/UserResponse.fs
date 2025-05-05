namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type UserResponse = {
    Id: string
    Username: string
}

module UserResponse =
    module Property =
        let [<Literal>] Id = "id"
        let [<Literal>] Username = "username"

    let decoder: Decoder<UserResponse> =
        Decode.object (fun get -> {
            Id = get.Required.Field Property.Id Decode.string
            Username = get.Required.Field Property.Username Decode.string
        })

    let encoder (v: UserResponse) =
        Encode.object [
            Property.Id, Encode.string v.Id
            Property.Username, Encode.string v.Username
        ]

    let fromDomain (v: User) = {
        Id = v.Id
        Username = v.Username
    }
