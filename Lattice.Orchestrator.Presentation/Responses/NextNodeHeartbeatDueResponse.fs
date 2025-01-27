namespace Lattice.Orchestrator.Presentation

open System
open System.Text.Json.Serialization

type NextHeartbeatDueResponse (nextHeartbeatDue) =
    [<JsonPropertyName "nextHeartbeatDue">]
    member _.NextHeartbeatDue: DateTime = nextHeartbeatDue

module NextHeartbeatDueResponse =
    let fromDomain (nextHeartbeatDueDate: DateTime) =
        NextHeartbeatDueResponse(nextHeartbeatDueDate)
