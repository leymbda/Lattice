namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open System.Text.Json.Serialization

type DisabledApplicationReasonResponse = {
    [<JsonPropertyName "disabledReasons">] DisabledReasons: int
}

module DisabledApplicationReasonResponse =
    let fromDomain (reasons: DisabledApplicationReason list) =
        {
            DisabledReasons = DisabledApplicationReason.toBitfield reasons
        }
