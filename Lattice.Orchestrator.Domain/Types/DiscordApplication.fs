namespace Lattice.Orchestrator.Domain

type PrivilegedIntents = {
    Presence: bool
    GuildMembers: bool
    MessageContent: bool
}

type ApplicationMember = {
    Id: string
    Role: ApplicationMemberRole
}

type DiscordApplication = {
    Id: string
    PrivilegedIntents: PrivilegedIntents
    Members: ApplicationMember list
}
