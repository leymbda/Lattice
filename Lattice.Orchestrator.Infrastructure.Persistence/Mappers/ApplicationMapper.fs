module Lattice.Orchestrator.Infrastructure.Persistence.ApplicationMapper

open Lattice.Orchestrator.Domain

let toDomain model =
    match model with
    | ApplicationModel.REGISTERED model ->
        Application.REGISTERED {
            Id = model.Id
            DiscordBotToken = model.DiscordBotToken
            DisabledReasons = model.DisabledReasons
        }

    | ApplicationModel.ACTIVATED model ->
        Application.ACTIVATED {
            Id = model.Id
            DiscordBotToken = model.DiscordBotToken
            Intents = model.Intents
            ProvisionedShardCount = model.ProvisionedShardCount
            Handler = Option.map HandlerMapper.toDomain model.Handler
            DisabledReasons = model.DisabledReasons
        }

let fromDomain application =
    match application with
    | Application.REGISTERED application ->
        ApplicationModel.REGISTERED {
            Id = application.Id
            Type = ApplicationModelType.REGISTERED
            DiscordBotToken = application.DiscordBotToken
            DisabledReasons = application.DisabledReasons
        }

    | Application.ACTIVATED application ->
        ApplicationModel.ACTIVATED {
            Id = application.Id
            Type = ApplicationModelType.ACTIVATED
            DiscordBotToken = application.DiscordBotToken
            Intents = application.Intents
            ProvisionedShardCount = application.ProvisionedShardCount
            Handler = Option.map HandlerMapper.fromDomain application.Handler
            DisabledReasons = application.DisabledReasons
        }
