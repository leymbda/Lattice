namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System.Threading.Tasks

type IPersistence =
    abstract SetUser: user: User -> Task<Result<User, unit>>

    abstract GetApp: appId: string -> Task<Result<App, unit>>
    abstract SetApp: app: App -> Task<Result<App, unit>>
    abstract RemoveApp: appId: string -> Task<Result<unit, unit>>
    