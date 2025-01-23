namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System

type RegisterNodeCommandError =
    | RegistrationFailed

module RegisterNodeCommand =
    let run (env: #IPersistence) = task {
        // Create node and save to db
        let id = Guid.NewGuid().ToString()

        let node = Node.create id DateTime.UtcNow

        match! env.UpsertNode node with
        | Error _ -> return Error RegisterNodeCommandError.RegistrationFailed
        | Ok node -> return Ok node
    }
