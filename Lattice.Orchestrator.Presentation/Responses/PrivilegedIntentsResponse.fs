namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type PrivilegedIntentsResponse = {
    [<JsonPropertyName "messageContent">] MessageContent: bool
    [<JsonPropertyName "messageContentLimited">] MessageContentLimited: bool
    [<JsonPropertyName "guildMembers">] GuildMembers: bool
    [<JsonPropertyName "guildMembersLimited">] GuildMembersLimited: bool
    [<JsonPropertyName "presence">] Presence: bool
    [<JsonPropertyName "presenceLimited">] PresenceLimited: bool
}

module PrivilegedIntentsResponse =
    let fromDomain (privilegedIntents: PrivilegedIntents) =
        {
            MessageContent = privilegedIntents.MessageContent
            MessageContentLimited = privilegedIntents.MessageContentLimited
            GuildMembers = privilegedIntents.GuildMembers
            GuildMembersLimited = privilegedIntents.GuildMembersLimited
            Presence = privilegedIntents.Presence
            PresenceLimited = privilegedIntents.PresenceLimited
        }
