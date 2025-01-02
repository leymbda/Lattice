[<AutoOpen>]
module Lattice.Orchestrator.Application.Persistence

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type GetApplicationById = string -> Task<Result<Application, unit>>

type UpsertApplication = Application -> Task<Result<Application, unit>>

type DeleteApplicationById = string -> Task<Result<unit, unit>>

type GetShardById = string -> Task<Result<Shard, unit>>

type GetShardsByApplicationId = string -> Task<Result<Shard list, unit>>

type UpsertShard = Shard -> Task<Result<Shard, unit>>

type DeleteShardById = string -> Task<Result<unit, unit>>
