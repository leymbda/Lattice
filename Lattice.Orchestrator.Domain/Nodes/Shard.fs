namespace Lattice.Orchestrator.Domain

open System

type ShardId = ShardId of applicationId: string * formulaId: int * numShards: int

module ShardId =
    let [<Literal>] SEPARATOR = ':'

    let create applicationId formulaId numShards =
        ShardId (applicationId, formulaId, numShards)

    let toString (ShardId (applicationId, formulaId, numShards)) =
        $"{applicationId}{SEPARATOR}{formulaId}{SEPARATOR}{numShards}"

    let fromString (str: string) =
        try
            match str.Split SEPARATOR with
            | [| applicationId; formulaId; numShards |] -> Some (ShardId (applicationId, int formulaId, int numShards))
            | _ -> None
        with | _ ->
            None

type Shard = {
    Id: ShardId
    ShutdownTime: DateTime option
}

module Shard =
    let create id =
        {
            Id = id
            ShutdownTime = None
        }

    let scheduleShutdown time (shard: Shard) =
        { shard with ShutdownTime = Some time }
