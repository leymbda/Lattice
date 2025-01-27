namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open System

type HeartbeatNodeCommandProps = {
    NodeId: Guid
}

type HeartbeatNodeCommandError =
    | NodeNotFound
    | HeartbeatFailed

module HeartbeatNodeCommand =
    let run (env: #IPersistence) (props: HeartbeatNodeCommandProps) = task {
        // Get node from database
        match! env.GetNodeById props.NodeId with
        | Error _ -> return Error HeartbeatNodeCommandError.NodeNotFound
        | Ok node ->
        
        // Do heartbeat then save node to db
        let updatedNode = Node.heartbeat DateTime.UtcNow node

        match! env.UpsertNode updatedNode with
        | Error _ -> return Error HeartbeatNodeCommandError.HeartbeatFailed
        | Ok node ->

        // Return due date for next heartbeat
        return Ok (node.LastHeartbeatAck.AddSeconds Node.NODE_LIFETIME_SECONDS)
    }
