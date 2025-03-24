namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain

type TeamMemberModel = {
    UserId: string
    Role: TeamMemberRole
}

type TeamCacheModel = {
    AppId: string
    Members: TeamMemberModel list
}

module TeamCacheModel =
    let toDomain (v: TeamCacheModel): Team =
        {
            AppId = v.AppId
            Members = v.Members |> List.map (fun m -> m.UserId, m.Role) |> Map.ofSeq
        }

    let fromDomain (v: Team): TeamCacheModel =
        {
            AppId = v.AppId
            Members = v.Members |> Map.toList |> List.map (fun (id, role) -> { UserId = id; Role = role })
        }
