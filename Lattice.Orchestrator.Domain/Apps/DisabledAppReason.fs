namespace Lattice.Orchestrator.Domain

type DisabledAppReason =
    | INVALID_DISCORD_BOT_TOKEN = (1 <<< 0)
    | INVALID_INTENTS           = (1 <<< 1)
    | NOT_ENOUGH_SHARDS         = (1 <<< 2)
    | BLACKLISTED               = (1 <<< 3)
    
module DisabledAppReason =
    let toBitfield (reasons: DisabledAppReason list) =
        reasons |> List.fold (fun acc r -> acc + int r) 0

    let fromBitfield (bitfield: int) =
        DisabledAppReason.GetValues()
        |> Array.toList
        |> List.filter (fun (t: DisabledAppReason) -> bitfield ||| (int t) = bitfield)
