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
    module Property =
        let [<Literal>] MessageContent = "messageContent"
        let [<Literal>] MessageContentLimited = "messageContentLimited"
        let [<Literal>] GuildMembers = "guildMembers"
        let [<Literal>] GuildMembersLimited = "guildMembersLimited"
        let [<Literal>] Presence = "presence"
        let [<Literal>] PresenceLimited = "presenceLimited"

    let decoder: Decoder<PrivilegedIntentsResponse> =
        Decode.object (fun get -> {
            MessageContent = get.Required.Field Property.MessageContent Decode.bool
            MessageContentLimited = get.Required.Field Property.MessageContentLimited Decode.bool
            GuildMembers = get.Required.Field Property.GuildMembers Decode.bool
            GuildMembersLimited = get.Required.Field Property.GuildMembersLimited Decode.bool
            Presence = get.Required.Field Property.Presence Decode.bool
            PresenceLimited = get.Required.Field Property.PresenceLimited Decode.bool
        })

    let encoder (v: PrivilegedIntentsResponse) =
        Encode.object [
            Property.MessageContent, Encode.bool v.MessageContent
            Property.MessageContentLimited, Encode.bool v.MessageContentLimited
            Property.GuildMembers, Encode.bool v.GuildMembers
            Property.GuildMembersLimited, Encode.bool v.GuildMembersLimited
            Property.Presence, Encode.bool v.Presence
            Property.PresenceLimited, Encode.bool v.PresenceLimited
        ]

    let fromDomain (v: PrivilegedIntents) = {
        MessageContent = v.MessageContent
        MessageContentLimited = v.MessageContentLimited
        GuildMembers = v.GuildMembers
        GuildMembersLimited = v.GuildMembersLimited
        Presence = v.Presence
        PresenceLimited = v.PresenceLimited
    }
