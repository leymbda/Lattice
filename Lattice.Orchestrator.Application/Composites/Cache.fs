namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type GetTeamError =
    | InvalidBotToken

module Cache =
    let getTeam (env: #IDiscord & #IPersistence & #ISecrets) (application: Application) = task {
        // Get from cache if available
        match! env.GetCachedApplicationTeam application.Id with
        | Ok team -> return Ok team
        | Error _ ->

        let botToken = application.EncryptedBotToken |> Aes.decrypt env.BotTokenEncryptionKey

        // Fetch from discord if not in cache
        match! env.GetApplicationInformation botToken with
        | None -> return Error GetTeamError.InvalidBotToken
        | Some discordApplication ->

        let members = Map.empty<string, TeamMemberRole> // TODO: Map app owner/team-members into here (requires new FSharp.Discord version)
        let team = Team.create application.Id members
        return Ok team
    }

// TODO: This module should be rewritten in a neater way. This is essentially more infra wrapping two infra projects,
//       and should be abstracted away to behave like a regular "getTeam" operation. Maybe there should be some kind of
//       differently named folder here in the application layer that reimplements the infra abstractions with the cache
//       behaviour.
