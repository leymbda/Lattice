namespace Lattice.Orchestrator.Domain

type TeamMemberRole =
    | OWNER = 0
    | ADMIN = 1
    | DEVELOPER = 2
    | READONLY = 3

type Team = {
    ApplicationId: string
    Members: Map<string, TeamMemberRole>
}

module Team =
    let create applicationId members =
        {
            ApplicationId = applicationId
            Members = members
        }
