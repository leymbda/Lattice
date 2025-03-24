namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

module TeamAdapter =
    let getTeam (env: #ICache & #IDiscord) applicationId botToken = task {
        // Get from cache if available
        match! env.GetTeam applicationId with
        | Ok team -> return Some team
        | Error _ ->

        // Fetch from discord if not in cache
        match! env.GetApplicationInformation botToken with
        | None -> return None
        | Some discordApplication ->

        let members = Map.empty<string, TeamMemberRole> // TODO: Map app owner/team-members into here (requires new FSharp.Discord version)
        let team = Team.create applicationId members
        return Some team
    }
