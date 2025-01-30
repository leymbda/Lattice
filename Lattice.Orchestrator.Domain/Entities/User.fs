namespace Lattice.Orchestrator.Domain

type User = {
    Id: string
    AccessToken: string
    RefreshToken: string
}

module User =
    let create id accessToken refreshToken = {
        Id = id
        AccessToken = accessToken
        RefreshToken = refreshToken
    }
