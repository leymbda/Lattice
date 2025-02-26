namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type DisabledApplicationReasonResponse = {
    DisabledReasons: int
}

module DisabledApplicationReasonResponse =
    let encoder (v: DisabledApplicationReasonResponse) =
        Encode.object [
            "disabledReasons", Encode.int v.DisabledReasons
        ]

    let fromDomain (reasons: DisabledApplicationReason list) = {
        DisabledReasons = DisabledApplicationReason.toBitfield reasons
    }
