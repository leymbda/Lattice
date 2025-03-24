namespace Lattice.Orchestrator.Domain

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
