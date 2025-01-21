namespace Lattice.Orchestrator.Domain

type Application = {
    Id:                    string
    DiscordBotToken:       string
    PrivilegedIntents:     PrivilegedIntents
    DisabledReasons:       int
    Intents:               int
    ProvisionedShardCount: int
    Handler:               Handler option
}

module Application =
    let create id discordBotToken privilegedIntents =
        {
            Id = id
            DiscordBotToken = discordBotToken
            PrivilegedIntents = privilegedIntents
            DisabledReasons = 0
            Intents = 0
            ProvisionedShardCount = 0
            Handler = None
        }
        
    let setDiscordBotToken discordBotToken (app: Application) =
        { app with DiscordBotToken = discordBotToken }

    let setPrivilegedIntents privilegedIntents (app: Application) =
        { app with PrivilegedIntents = privilegedIntents }
        
    let addDisabledReason reason (app: Application) =
        { app with DisabledReasons = app.DisabledReasons ||| reason }

    let removeDisabledReason reason (app: Application) =
        { app with DisabledReasons = app.DisabledReasons &&& (~~~reason) }

    let addIntent intent (app: Application) =
        { app with Intents = app.Intents ||| intent }

    let removeIntent intent (app: Application) =
        { app with Intents = app.Intents &&& (~~~intent) }

    let setIntents intents (app: Application) =
        { app with Intents = intents }

    let setProvisionedShardCount provisionedShardCount (app: Application) =
        { app with ProvisionedShardCount = provisionedShardCount }

    let setHandler handler (app: Application) =
        { app with Handler = Some handler }

    let removeHandler (app: Application) =
        { app with Handler = None }
