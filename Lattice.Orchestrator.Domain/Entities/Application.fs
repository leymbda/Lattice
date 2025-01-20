namespace Lattice.Orchestrator.Domain

type Application =
    | REGISTERED  of RegisteredApplication
    | ACTIVATED   of ActivatedApplication

and RegisteredApplication = {
    Id:              string
    DiscordBotToken: string
    DisabledReasons: int
}

and ActivatedApplication = {
    Id:                    string
    DiscordBotToken:       string
    Intents:               int
    ProvisionedShardCount: int
    DisabledReasons:       int
    Handler:               Handler option
}
