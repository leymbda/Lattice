namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type PrivilegedIntentsResponse = {
    MessageContent: bool
    MessageContentLimited: bool
    GuildMembers: bool
    GuildMembersLimited: bool
    Presence: bool
    PresenceLimited: bool
}

module PrivilegedIntentsResponse =
    let encoder (v: PrivilegedIntentsResponse) =
        Encode.object [
            "messageContent", Encode.bool v.MessageContent
            "messageContentLimited", Encode.bool v.MessageContentLimited
            "guildMembers", Encode.bool v.GuildMembers
            "guildMembersLimited", Encode.bool v.GuildMembersLimited
            "presence", Encode.bool v.Presence
            "presenceLimited", Encode.bool v.PresenceLimited
        ]

    let fromDomain (v: PrivilegedIntents) = {
        MessageContent = v.MessageContent
        MessageContentLimited = v.MessageContentLimited
        GuildMembers = v.GuildMembers
        GuildMembersLimited = v.GuildMembersLimited
        Presence = v.Presence
        PresenceLimited = v.PresenceLimited
    }
