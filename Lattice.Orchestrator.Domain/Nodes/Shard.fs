namespace Lattice.Orchestrator.Domain

open System

type ShardId = ShardId of appId: string * formulaId: int * numShards: int

module ShardId =
    let [<Literal>] SEPARATOR = ':'

    let create appId formulaId numShards =
        ShardId (appId, formulaId, numShards)

    let toString (ShardId (appId, formulaId, numShards)) =
        $"{appId}{SEPARATOR}{formulaId}{SEPARATOR}{numShards}"

    let fromString (str: string) =
        try
            match str.Split SEPARATOR with
            | [| appId; formulaId; numShards |] -> Some (ShardId (appId, int formulaId, int numShards))
            | _ -> None
        with | _ ->
            None

type ShardState =
    | NotStarted
    | Starting of next: Guid * startAt: DateTime
    | Active of current: Guid
    | Transferring of current: Guid * next: Guid * transferAt: DateTime
    | ShuttingDown of current: Guid * shutdownAt: DateTime
    | Shutdown of shutdownAt: DateTime

type Shard = {
    Id: ShardId
    Instances: (DateTime * Guid option) list
}

module Shard =
    let create appId formulaId numShards =
        {
            Id = ShardId.create appId formulaId numShards
            Instances = []
        }

    let addInstance nodeId createAt shard =
        { shard with Instances = (createAt, Some nodeId) :: shard.Instances }

    let shutdown shutdownAt shard =
        { shard with Instances =  (shutdownAt, None) :: shard.Instances }

    let getState currentTime shard =
        match shard with
        | { Instances = [] } -> NotStarted
        | { Instances = (createAt, Some current) :: _ } when createAt <= currentTime -> Active current
        | { Instances = (transferAt, Some next) :: (_, Some current) :: _ } when transferAt > currentTime -> Transferring (current, next, transferAt)
        | { Instances = (createAt, Some next) :: _ } -> Starting (next, createAt)
        | { Instances = (shutdownAt, None) :: (_, Some current) :: _ } when shutdownAt > currentTime -> ShuttingDown (current, shutdownAt)
        | { Instances = (shutdownAt, None) :: _ } -> Shutdown shutdownAt

    // TODO: If multiple shutdowns are requested, this will treat it as already shutdown even if timer not reached (potential bug)
