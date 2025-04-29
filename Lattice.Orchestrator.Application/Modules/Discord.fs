namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open Lattice.Orchestrator.Domain

module PrivilegedIntents =
    let fromFlags (flags: ApplicationFlag list): PrivilegedIntents =
        {
            MessageContent = List.contains ApplicationFlag.GATEWAY_MESSAGE_CONTENT flags
            MessageContentLimited = List.contains ApplicationFlag.GATEWAY_MESSAGE_CONTENT_LIMITED flags
            GuildMembers = List.contains ApplicationFlag.GATEWAY_GUILD_MEMBERS flags
            GuildMembersLimited = List.contains ApplicationFlag.GATEWAY_GUILD_MEMBERS_LIMITED flags
            Presence = List.contains ApplicationFlag.GATEWAY_PRESENCE flags
            PresenceLimited = List.contains ApplicationFlag.GATEWAY_PRESENCE_LIMITED flags
        }

module Team =
    let fromApplication (application: Application): Team option =
        match application.Team, application.Owner with
        | Some team, Some owner ->
            let applyOwnerRole userId role =
                match role with
                | _ when userId = owner.Id -> TeamMemberRole.OWNER
                | TeamMemberRoleType.ADMIN -> TeamMemberRole.ADMIN
                | TeamMemberRoleType.DEVELOPER -> TeamMemberRole.DEVELOPER
                | TeamMemberRoleType.READONLY -> TeamMemberRole.READONLY

            let members =
                team.Members
                |> List.map (fun m -> m.User.Id, applyOwnerRole m.User.Id m.Role)
                |> Map.ofList

            Some {
                AppId = application.Id
                Members = members
            }

        | _ -> None
