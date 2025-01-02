namespace Lattice.Orchestrator.Domain

type DisabledApplicationReason =
    | INVALID_DISCORD_BOT_TOKEN     = (1 <<< 0)
    | INVALID_INTENTS               = (1 <<< 1)
    | NOT_ENOUGH_PROVISIONED_SHARDS = (1 <<< 2)
    | BLACKLISTED                   = (1 <<< 3)
