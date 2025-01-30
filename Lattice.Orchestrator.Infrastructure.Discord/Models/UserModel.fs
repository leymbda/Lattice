namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Types
open Lattice.Orchestrator.Application

type UserModel = FSharp.Discord.Types.User

module UserModel =
    let toDomain (app: UserModel): IDiscordUser =
        { new IDiscordUser with
            member _.Id = app.Id }
