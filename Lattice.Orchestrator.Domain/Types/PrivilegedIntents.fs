namespace Lattice.Orchestrator.Domain

type PrivilegedIntents = {
    MessageContent: bool
    MessageContentLimited: bool
    GuildMembers: bool
    GuildMembersLimited: bool
    Presence: bool
    PresenceLimited: bool
}
