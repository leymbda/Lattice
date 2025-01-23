namespace Lattice.Orchestrator.Infrastructure.Tasks

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open System.Threading.Tasks

type CheckNodeHealthCron (env: IEnv) =
    [<Function "CheckNodeHealthCron">]
    let run (
        [<TimerTrigger "0 */1 * * *">] timer: TimerInfo
    ) = task {
        do! DeleteExpiredNodesCommand.run env :> Task
    }
