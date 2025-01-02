namespace Lattice.Orchestrator.Domain

type Application =
    | REGISTERED of RegisteredApplication
    | ACTIVATED  of ActivatedApplication

and RegisteredApplication = {
    Id:              string
    DiscordBotToken: string
}

and ActivatedApplication = {
    Id:                    string
    DiscordBotToken:       string
    Intents:               int
    ProvisionedShardCount: int
    Handler:               Handler
    DisabledReasons:       int
}
