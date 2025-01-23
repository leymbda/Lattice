namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System
open System.Threading.Tasks

type DeleteExpiredNodesCommandError =
    | LookupFailed

module DeleteExpiredNodesCommand =
    let run (env: #IPersistence & #ITask) = task {
        // Find all nodes that haven't met the heartbeat requirements
        match! env.GetExpiredNodes Node.NODE_LIFETIME_SECONDS DateTime.UtcNow with
        | Error _ -> return Error DeleteExpiredNodesCommandError.LookupFailed
        | Ok nodes ->
        
        // Initiate shutdown tasks for all expired nodes
        do! nodes
            |> List.map (fun node -> env.BeginNodeShutdownTask node.Id :> Task)
            |> Task.WhenAll

        return Ok ()
    }
