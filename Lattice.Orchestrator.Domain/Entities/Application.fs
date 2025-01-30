namespace Lattice.Orchestrator.Domain

type Application = {
    Id:                    string
    EncryptedBotToken:     string
    PrivilegedIntents:     PrivilegedIntents
    DisabledReasons:       DisabledApplicationReason list
    Intents:               int
    ProvisionedShardCount: int
    Handler:               Handler option
}

module Application =
    let create id encryptedBotToken privilegedIntents =
        {
            Id = id
            EncryptedBotToken = encryptedBotToken
            PrivilegedIntents = privilegedIntents
            DisabledReasons = []
            Intents = 0
            ProvisionedShardCount = 0
            Handler = None
        }
        
    let setEncryptedBotToken encryptedBotToken (app: Application) =
        { app with EncryptedBotToken = encryptedBotToken }

    let setPrivilegedIntents privilegedIntents (app: Application) =
        { app with PrivilegedIntents = privilegedIntents }
        
    let addDisabledReason reason (app: Application) =
        { app with DisabledReasons = app.DisabledReasons |> List.append [reason] |> List.distinct }

    let removeDisabledReason reason (app: Application) =
        { app with DisabledReasons = app.DisabledReasons |> List.filter (fun r -> r <> reason) }
        
    let setDisabledReasons reasons (app: Application) =
        { app with DisabledReasons = reasons }

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
