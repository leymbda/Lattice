namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System
open System.Text.Json.Serialization

type NodeResponse (id, lastHeartbeatAck) =
    [<JsonPropertyName "id">]
    member _.Id: string = id

    [<JsonPropertyName "lastHeartbeatAck">]
    member _.LastHeartbeatAck: DateTime = lastHeartbeatAck

module NodeResponse =
    let fromDomain (node: Node) =
        NodeResponse(node.Id, node.LastHeartbeatAck)
