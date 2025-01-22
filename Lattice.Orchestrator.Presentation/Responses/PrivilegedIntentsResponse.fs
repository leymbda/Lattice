namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type PrivilegedIntentsResponse (messageContent, messageContentLimited, guildMembers, guildMembersLimited, presence, presenceLimited) =
    [<JsonPropertyName "messageContent">]
    member _.MessageContent: bool = messageContent

    [<JsonPropertyName "messageContentLimited">]
    member _.MessageContentLimited: bool = messageContentLimited

    [<JsonPropertyName "guildMembers">]
    member _.GuildMembers: bool = guildMembers

    [<JsonPropertyName "guildMembersLimited">]
    member _.GuildMembersLimited: bool = guildMembersLimited

    [<JsonPropertyName "presence">]
    member _.Presence: bool = presence

    [<JsonPropertyName "presenceLimited">]
    member _.PresenceLimited: bool = presenceLimited

module PrivilegedIntentsResponse =
    let fromDomain (privilegedIntents: PrivilegedIntents) =
        PrivilegedIntentsResponse(
            privilegedIntents.MessageContent,
            privilegedIntents.MessageContentLimited,
            privilegedIntents.GuildMembers,
            privilegedIntents.GuildMembersLimited,
            privilegedIntents.Presence,
            privilegedIntents.PresenceLimited
        )
