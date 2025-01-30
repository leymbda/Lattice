namespace Lattice.Orchestrator.Domain

type User = {
    Id: string
    EncryptedAccessToken: string
    EncryptedRefreshToken: string
}

module User =
    let create id encryptedAccessToken encryptedRefreshToken = {
        Id = id
        EncryptedAccessToken = encryptedAccessToken
        EncryptedRefreshToken = encryptedRefreshToken
    }

    let setEncryptedAccessToken encryptedAccessToken (user: User) =
        { user with EncryptedAccessToken = encryptedAccessToken }

    let setEncryptedRefreshToken encryptedRefreshToken (user: User) =
        { user with EncryptedRefreshToken = encryptedRefreshToken }
