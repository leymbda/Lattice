namespace Lattice.Orchestrator.Domain

type User = {
    Id: string
    Username: string
}

module User =
    let create id username = {
        Id = id
        Username = username
    }

    let setUsername username (user: User) =
        { user with Username = username }
