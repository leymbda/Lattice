namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type ICache =
    abstract GetTeam: applicationId: string -> Task<Result<Team, unit>>
    abstract SetTeam: team: Team -> Task<Result<Team, unit>>
    abstract RemoveTeam: applicationId: string -> Task<Result<unit, unit>>
