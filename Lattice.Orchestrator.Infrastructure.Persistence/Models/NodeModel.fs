namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System
open System.Text.Json.Serialization

type NodeModel = {
    [<JsonPropertyName "id">] Id: string
    [<JsonPropertyName "lastHeartbeatAck">] LastHeartbeatAck: int
}

module NodeModel =
    let toDomain (model: NodeModel): Node =
        {
            Id = model.Id
            LastHeartbeatAck = DateTime.UnixEpoch.AddSeconds(model.LastHeartbeatAck)
        }

    let fromDomain (node: Node): NodeModel =
        {
            Id = node.Id
            LastHeartbeatAck = int (node.LastHeartbeatAck - DateTime.UnixEpoch).TotalSeconds
        }
