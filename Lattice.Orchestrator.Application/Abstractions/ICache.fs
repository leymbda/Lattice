namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type ICache =
    abstract GetTeam: appId: string -> Task<Result<Team, unit>>
    abstract SetTeam: team: Team -> Task<Result<Team, unit>>
    abstract RemoveTeam: appId: string -> Task<Result<unit, unit>>
