namespace Lattice.Orchestrator.Contracts

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
    let decoder: Decoder<PrivilegedIntentsResponse> =
        Decode.object (fun get -> {
            MessageContent = get.Required.Field "messageContent" Decode.bool
            MessageContentLimited = get.Required.Field "messageContentLimited" Decode.bool
            GuildMembers = get.Required.Field "guildMembers" Decode.bool
            GuildMembersLimited = get.Required.Field "guildMembersLimited" Decode.bool
            Presence = get.Required.Field "presence" Decode.bool
            PresenceLimited = get.Required.Field "presenceLimited" Decode.bool
        })

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
