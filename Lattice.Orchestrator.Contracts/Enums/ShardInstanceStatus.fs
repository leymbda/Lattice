namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain

type ShardInstanceStatus =
    | NotStarted = 0
    | Starting = 1
    | Active = 2
    | ShuttingDown = 3
    | Shutdown = 4

module ShardInstanceStatus =
    let fromDomain (state: ShardInstanceState) =
        match state with
        | ShardInstanceState.NotStarted -> ShardInstanceStatus.NotStarted
        | ShardInstanceState.Starting _ -> ShardInstanceStatus.Starting
        | ShardInstanceState.Active -> ShardInstanceStatus.Active
        | ShardInstanceState.ShuttingDown _ -> ShardInstanceStatus.ShuttingDown
        | ShardInstanceState.Shutdown _ -> ShardInstanceStatus.ShuttingDown
