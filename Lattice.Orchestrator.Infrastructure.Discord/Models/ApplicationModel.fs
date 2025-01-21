namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Types
open Lattice.Orchestrator.Application

type ApplicationModel = FSharp.Discord.Types.Application

module ApplicationModel =
    let toDomain (app: ApplicationModel): IDiscordApplication =
        let hasFlag = fun flag -> (Option.defaultValue 0 app.Flags &&& int flag) = int flag

        { new IDiscordApplication with
            member _.Id = app.Id
            member _.HasMessageContentIntent = hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT
            member _.HasMessageContentLimitedIntent = hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED
            member _.HasGuildMembersIntent = hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS
            member _.HasGuildMembersLimitedIntent = hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED
            member _.HasPresenceIntent = hasFlag ApplicationFlag.GATEWAY_PRESENCE
            member _.HasPresenceLimitedIntent = hasFlag ApplicationFlag.GATEWAY_PRESENCE_LIMITED }
