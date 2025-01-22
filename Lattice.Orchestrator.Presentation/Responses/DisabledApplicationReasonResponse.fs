namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type DisabledApplicationReasonResponse (disabledReasons) =
    [<JsonPropertyName "disabledReasons">]
    member _.DisabledReasons: int = disabledReasons

module DisabledApplicationReasonResponse =
    let fromDomain (reasons: DisabledApplicationReason list) =
        DisabledApplicationReasonResponse(DisabledApplicationReason.toBitfield reasons)
