namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type PaginatedResponse<'a> = {
    [<JsonPropertyName "items">] Items: 'a list
    [<JsonPropertyName "totalCount">] TotalCount: int
}
