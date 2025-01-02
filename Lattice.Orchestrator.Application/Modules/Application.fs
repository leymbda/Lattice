module Lattice.Orchestrator.Application.Application

open Lattice.Orchestrator.Domain

let register id discordBotToken =
    {
        Id = id
        DiscordBotToken = discordBotToken
    }

let activate intents provisionedShardCount handler (app: RegisteredApplication) =
    {
        Id = app.Id
        DiscordBotToken = app.DiscordBotToken
        Intents = intents
        ProvisionedShardCount = provisionedShardCount
        Handler = handler
        DisabledReasons = 0
    }

let addIntent intent (app: ActivatedApplication) =
    { app with Intents = app.Intents ||| intent }

let removeIntent intent (app: ActivatedApplication) =
    { app with Intents = app.Intents &&& (~~~intent) }

let setIntents intents (app: ActivatedApplication) =
    { app with Intents = intents }

let setProvisionedShardCount provisionedShardCount (app: ActivatedApplication) =
    { app with ProvisionedShardCount = provisionedShardCount }

let setHandler handler (app: ActivatedApplication) =
    { app with Handler = Some handler }

let removeHandler (app: ActivatedApplication) =
    { app with Handler = None }

let addDisabledReason reason (app: ActivatedApplication) =
    { app with DisabledReasons = app.DisabledReasons ||| reason }

let removeDisabledReason reason (app: ActivatedApplication) =
    { app with DisabledReasons = app.DisabledReasons &&& (~~~reason) }
