namespace Lattice.Orchestrator.Domain

type DiscordApplication = {
    Id: string
    PrivilegedIntents: PrivilegedIntents
    Members: (string * ApplicationMemberRole) seq option
}
