namespace Lattice.Orchestrator.Application

open FSharp.Discord.Types
open System.Threading.Tasks

type IDiscord =
    abstract GetUserInformation: accessToken: string -> Task<User option>
    abstract GetApplicationInformation: botToken: string -> Task<Application option>
