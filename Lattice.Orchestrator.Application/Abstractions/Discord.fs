[<AutoOpen>]
module Lattice.Orchestrator.Application.Discord

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type GetApplicationInformation = string -> Task<Result<DiscordApplication, unit>>
