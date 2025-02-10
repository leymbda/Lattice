namespace Lattice.Orchestrator.Presentation

type SecretsOptions () =
    member val DiscordClientId = "" with get
    member val DiscordClientSecret = "" with get

    member val UserAccessTokenEncryptionKey = "" with get
    member val UserRefreshTokenEncryptionKey = "" with get
    member val BotTokenEncryptionKey = "" with get

    member val JwtHashingKey = "" with get
