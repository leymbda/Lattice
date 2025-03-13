namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain

type TeamMemberModel = {
    UserId: string
    Role: TeamMemberRole
}

type TeamCacheModel = {
    ApplicationId: string
    Members: TeamMemberModel list
}

module TeamCacheModel =
    let toDomain (v: TeamCacheModel): Team =
        {
            ApplicationId = v.ApplicationId
            Members = v.Members |> List.map (fun m -> m.UserId, m.Role) |> Map.ofSeq
        }

    let fromDomain (v: Team): TeamCacheModel =
        {
            ApplicationId = v.ApplicationId
            Members = v.Members |> Map.toList |> List.map (fun (id, role) -> { UserId = id; Role = role })
        }
