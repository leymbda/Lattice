namespace Lattice.Orchestrator.Infrastructure.Persistence

open Lattice.Orchestrator.Domain
open System
open System.Text.Json.Serialization

type NodeModel = {
    [<JsonPropertyName "id">] Id: Guid
    [<JsonPropertyName "shards">] Shards: Guid list
}

module NodeModel =
    let toDomain (model: NodeModel): Node = {
        Id = model.Id
        Shards = model.Shards
    }

    let fromDomain (node: Node): NodeModel = {
        Id = node.Id
        Shards = node.Shards
    }
