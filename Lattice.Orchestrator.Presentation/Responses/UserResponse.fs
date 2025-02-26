namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type UserResponse = {
    Id: string
}

module UserResponse =
    let encoder (v: UserResponse) =
        Encode.object [
            "id", Encode.string v.Id
        ]

    let fromDomain (v: User) = {
        Id = v.Id
    }
