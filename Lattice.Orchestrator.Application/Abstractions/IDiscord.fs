namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type IDiscord =
    abstract GetApplicationInformation: token: string -> Task<DiscordApplication option>
