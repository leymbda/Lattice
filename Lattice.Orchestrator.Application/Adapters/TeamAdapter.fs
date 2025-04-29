namespace Lattice.Orchestrator.Application

open System.Threading.Tasks

module TeamAdapter =
    let getTeam (env: #ICache & #IDiscord) appId botToken = task {
        // Get from cache if available
        match! env.GetTeam appId with
        | Ok team -> return Some team
        | Error _ ->

        // Fetch from discord if not in cache
        match! env.GetApplicationInformation botToken with
        | None -> return None
        | Some application ->

        match Team.fromApplication application with
        | None -> return None
        | Some team ->

        // Save team to cache
        do!
            env.SetTeam team
            |> Task.wait

        // Return team on success
        return Some team
    }
