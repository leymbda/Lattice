namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System

type RegisterNodeCommandError =
    | RegistrationFailed

module RegisterNodeCommand =
    let run (env: #IPersistence) = task {
        // Create node and save to db
        let node = Node.create (Guid.NewGuid())

        match! env.UpsertNode node with
        | Error _ -> return Error RegisterNodeCommandError.RegistrationFailed
        | Ok node -> return Ok node

        // TODO: Start durable entity for node
    }
