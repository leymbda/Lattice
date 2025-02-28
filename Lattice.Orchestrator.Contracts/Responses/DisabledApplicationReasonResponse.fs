namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type DisabledApplicationReasonResponse = {
    DisabledReasons: int
}

module DisabledApplicationReasonResponse =
    let decoder: Decoder<DisabledApplicationReasonResponse> =
        Decode.object (fun get -> {
            DisabledReasons = get.Required.Field "disabledReasons" Decode.int
        })

    let encoder (v: DisabledApplicationReasonResponse) =
        Encode.object [
            "disabledReasons", Encode.int v.DisabledReasons
        ]

    let fromDomain (reasons: DisabledApplicationReason list) = {
        DisabledReasons = DisabledApplicationReason.toBitfield reasons
    }
