namespace Lattice.Orchestrator.Infrastructure.Discord

open FSharp.Discord.Types
open Lattice.Orchestrator.Application

type UserModel = FSharp.Discord.Types.User

module UserModel =
    let toDomain (v: UserModel): IDiscordUser =
        { new IDiscordUser with
            member _.Id = v.Id
            member _.Username = v.Username }
