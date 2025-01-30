namespace Lattice.Orchestrator.Application

type ISecrets =
    abstract ClientId: string
    abstract ClientSecret: string

    abstract UserAccessTokenEncryptionKey: string
    abstract UserRefreshTokenEncryptionKey: string
    abstract BotTokenEncryptionKey: string
