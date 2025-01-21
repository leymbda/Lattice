namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

type ApplicationModel = FSharp.Discord.Types.Application

module ApplicationModel =
    let toDomain (app: ApplicationModel): DiscordApplication =
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
            Members =
                List.empty<ApplicationMember>
                |> List.append (
                    app.Team
                    |> Option.map (fun t ->
                        t.Members
                        |> List.filter (fun m -> m.MembershipState = MembershipState.ACCEPTED)
                        |> List.map (fun m -> { Id = m.User.Id; Role = getRole t m }))
                    |> Option.defaultValue [])
                |> List.append (
                    app.Owner
                    |> Option.map (fun o -> { Id = o.Id; Role = ApplicationMemberRole.OWNER })
                    |> Option.toList)
                |> List.distinctBy (_.Id)
        }
