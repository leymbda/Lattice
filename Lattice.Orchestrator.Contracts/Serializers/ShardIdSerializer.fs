namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

module ShardId =
    let decoder: Decoder<ShardId> =
        Decode.string
        |> Decode.andThen(fun v ->
            match ShardId.fromString v with
            | None -> Decode.fail "Invalid ShardId"
            | Some shardId -> Decode.succeed shardId
        )

    let encoder (v: ShardId) =
        ShardId.toString v
        |> Encode.string
