namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type PrivilegedIntentsModel = {
    [<JsonPropertyName "messageContent">] MessageContent: bool
    [<JsonPropertyName "messageContentLimited">] MessageContentLimited: bool
    [<JsonPropertyName "guildMembers">] GuildMembers: bool
    [<JsonPropertyName "guildMembersLimited">] GuildMembersLimited: bool
    [<JsonPropertyName "presence">] Presence: bool
    [<JsonPropertyName "presenceLimited">] PresenceLimited: bool
}

module PrivilegedIntentsModel =
    let toDomain (model: PrivilegedIntentsModel): PrivilegedIntents =
        {
            MessageContent = model.MessageContent
            MessageContentLimited = model.MessageContentLimited
            GuildMembers = model.GuildMembers
            GuildMembersLimited = model.GuildMembersLimited
            Presence = model.Presence
            PresenceLimited = model.PresenceLimited
        }

    let fromDomain (privilegedIntents: PrivilegedIntents): PrivilegedIntentsModel =
        {
            MessageContent = privilegedIntents.MessageContent
            MessageContentLimited = privilegedIntents.MessageContentLimited
            GuildMembers = privilegedIntents.GuildMembers
            GuildMembersLimited = privilegedIntents.GuildMembersLimited
            Presence = privilegedIntents.Presence
            PresenceLimited = privilegedIntents.PresenceLimited
        }
