module Lattice.Orchestrator.Infrastructure.Discord.DiscordApplicationMapper

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

let toDomain (app: FSharp.Discord.Types.Application): DiscordApplication =
    let hasFlag = fun flag -> (Option.defaultValue 0 app.Flags &&& int flag) = int flag

    let getRole (team: Team) (teamMember: TeamMember) =
        match team, teamMember with
        | { OwnerUserId = ownerId }, { User = { Id = userId } } when ownerId = userId -> ApplicationMemberRole.OWNER
        | _, { Role = "admin" } -> ApplicationMemberRole.ADMIN
        | _, { Role = "developer" } -> ApplicationMemberRole.DEVELOPER
        | _, { Role = "read-only" } -> ApplicationMemberRole.READONLY
        | _ -> ApplicationMemberRole.READONLY // Should never occur, caused by poorly formed response from Discord

    {
        Id = app.Id
        PrivilegedIntents = {
            Presence =
                match app.ApproximateGuildCount with
                | Some v when v < 100 -> hasFlag ApplicationFlag.GATEWAY_PRESENCE_LIMITED || hasFlag ApplicationFlag.GATEWAY_PRESENCE
                | _ -> hasFlag ApplicationFlag.GATEWAY_PRESENCE
            GuildMembers =
                match app.ApproximateGuildCount with
                | Some v when v < 100 -> hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED || hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS
                | _ -> hasFlag ApplicationFlag.GATEWAY_GUILD_MEMBERS
            MessageContent =
                match app.ApproximateGuildCount with
                | Some v when v < 100 -> hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED || hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT
                | _ -> hasFlag ApplicationFlag.GATEWAY_MESSAGE_CONTENT
        }
        Members = app.Team |> Option.map (fun t -> t.Members |> Seq.map (fun m -> m.User.Id, getRole t m))
    }
