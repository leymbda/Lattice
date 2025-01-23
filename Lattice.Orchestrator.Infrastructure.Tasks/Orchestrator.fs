module Lattice.Orchestrator.Infrastructure.Tasks.Orchestrator

open Microsoft.DurableTask.Client
open System.Threading.Tasks

let beginNodeShutdownTask (durableTaskClient: DurableTaskClient) id = task {
    do! durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof NodeShutdownOrchestration, { Id = id }) :> Task
}
