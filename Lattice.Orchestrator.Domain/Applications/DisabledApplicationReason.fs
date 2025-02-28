namespace Lattice.Orchestrator.Domain

type DisabledApplicationReason =
    | INVALID_DISCORD_BOT_TOKEN = (1 <<< 0)
    | INVALID_INTENTS           = (1 <<< 1)
    | NOT_ENOUGH_SHARDS         = (1 <<< 2)
    | BLACKLISTED               = (1 <<< 3)
    
module DisabledApplicationReason =
    let toBitfield (reasons: DisabledApplicationReason list) =
        reasons |> List.fold (fun acc r -> acc + int r) 0

    let fromBitfield (bitfield: int) =
        DisabledApplicationReason.GetValues()
        |> Array.toList
        |> List.filter (fun (t: DisabledApplicationReason) -> bitfield ||| (int t) = bitfield)
