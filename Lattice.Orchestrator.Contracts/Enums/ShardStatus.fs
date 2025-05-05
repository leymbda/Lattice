namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain

type ShardStatus =
    | NotStarted = 0
    | Starting = 1
    | Active = 2
    | Transferring = 3
    | ShuttingDown = 4
    | Shutdown = 5

module ShardStatus =
    let fromDomain (state: ShardState) =
        match state with
        | ShardState.NotStarted -> ShardStatus.NotStarted
        | ShardState.Starting _ -> ShardStatus.Starting
        | ShardState.Active _ -> ShardStatus.Active
        | ShardState.Transferring _ -> ShardStatus.Transferring
        | ShardState.ShuttingDown _ -> ShardStatus.ShuttingDown
        | ShardState.Shutdown _ -> ShardStatus.ShuttingDown
