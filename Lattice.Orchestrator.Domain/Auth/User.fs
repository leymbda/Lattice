namespace Lattice.Orchestrator.Domain

type User = {
    Id: string
    Username: string
    EncryptedAccessToken: string
    EncryptedRefreshToken: string
}

module User =
    let create id username encryptedAccessToken encryptedRefreshToken = {
        Id = id
        Username = username
        EncryptedAccessToken = encryptedAccessToken
        EncryptedRefreshToken = encryptedRefreshToken
    }

    let setUsername username (user: User) =
        { user with Username = username }

    let setEncryptedAccessToken encryptedAccessToken (user: User) =
        { user with EncryptedAccessToken = encryptedAccessToken }

    let setEncryptedRefreshToken encryptedRefreshToken (user: User) =
        { user with EncryptedRefreshToken = encryptedRefreshToken }
