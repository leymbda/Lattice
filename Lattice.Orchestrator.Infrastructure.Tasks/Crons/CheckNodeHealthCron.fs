namespace Lattice.Orchestrator.Infrastructure.Tasks

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open System.Threading.Tasks

type CheckNodeHealthCron (env: IEnv) =
    [<Function(nameof CheckNodeHealthCron)>]
    member _.Run (
        [<TimerTrigger "0 */1 * * *">] ctx: FunctionContext
    ) = task {
        do! DeleteExpiredNodesCommand.run env :> Task
    }
