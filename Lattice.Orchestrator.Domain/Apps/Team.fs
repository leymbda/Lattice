namespace Lattice.Orchestrator.Domain

type TeamMemberRolePermission =
    | VIEW
    | MODIFY
    | ADMINISTRATE

type TeamMemberRole =
    | OWNER = 0
    | ADMIN = 1
    | DEVELOPER = 2
    | READONLY = 3

type Team = {
    AppId: string
    Members: Map<string, TeamMemberRole>
}

module Team =
    let create appId members =
        {
            AppId = appId
            Members = members
        }

    let getMemberRole userId (team: Team) =
        team.Members
        |> Map.tryFind userId

    let checkPermission userId permission (team: Team) =
        let role = getMemberRole userId team
        
        match permission, role with
        | TeamMemberRolePermission.VIEW, Some _ -> true
        | TeamMemberRolePermission.MODIFY, Some (TeamMemberRole.OWNER | TeamMemberRole.ADMIN | TeamMemberRole.DEVELOPER) -> true
        | TeamMemberRolePermission.ADMINISTRATE, Some (TeamMemberRole.OWNER | TeamMemberRole.ADMIN) -> true
        | _ -> false
