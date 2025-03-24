namespace Lattice.Orchestrator.Domain

type PrivilegedIntents = {
    MessageContent: bool
    MessageContentLimited: bool
    GuildMembers: bool
    GuildMembersLimited: bool
    Presence: bool
    PresenceLimited: bool
}

type App = {
    Id:                string
    EncryptedBotToken: string
    PrivilegedIntents: PrivilegedIntents
    DisabledReasons:   DisabledApplicationReason list
    Intents:           int
    ShardCount:        int
    Handler:           Handler option
}

module App =
    let create id encryptedBotToken privilegedIntents =
        {
            Id = id
            EncryptedBotToken = encryptedBotToken
            PrivilegedIntents = privilegedIntents
            DisabledReasons = []
            Intents = 0
            ShardCount = 0
            Handler = None
        }
        
    let setEncryptedBotToken encryptedBotToken (app: App) =
        { app with EncryptedBotToken = encryptedBotToken }

    let setPrivilegedIntents privilegedIntents (app: App) =
        { app with PrivilegedIntents = privilegedIntents }
        
    let addDisabledReason reason (app: App) =
        { app with DisabledReasons = app.DisabledReasons |> List.append [reason] |> List.distinct }

    let removeDisabledReason reason (app: App) =
        { app with DisabledReasons = app.DisabledReasons |> List.filter (fun r -> r <> reason) }
        
    let setDisabledReasons reasons (app: App) =
        { app with DisabledReasons = reasons }

    let addIntent intent (app: App) =
        { app with Intents = app.Intents ||| intent }

    let removeIntent intent (app: App) =
        { app with Intents = app.Intents &&& (~~~intent) }

    let setIntents intents (app: App) =
        { app with Intents = intents }

    let setShardCount shardCount (app: App) =
        { app with ShardCount = shardCount }

    let setHandler handler (app: App) =
        { app with Handler = Some handler }

    let removeHandler (app: App) =
        { app with Handler = None }
