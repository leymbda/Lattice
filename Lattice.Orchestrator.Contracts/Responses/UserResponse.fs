namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type UserResponse = {
    Id: string
    Username: string
}

module UserResponse =
    let encoder (v: UserResponse) =
        Encode.object [
            "id", Encode.string v.Id
            "username", Encode.string v.Username
        ]

    let fromDomain (v: User) = {
        Id = v.Id
        Username = v.Username
    }
