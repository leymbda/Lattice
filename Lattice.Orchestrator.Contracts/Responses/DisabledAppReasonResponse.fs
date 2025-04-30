namespace Lattice.Orchestrator.Contracts

open Lattice.Orchestrator.Domain
open Thoth.Json.Net

type DisabledAppReasonResponse = {
    DisabledReasons: int
}

module DisabledAppReasonResponse =
    module Property =
        let [<Literal>] DisabledReasons = "disabledReasons"

    let decoder: Decoder<DisabledAppReasonResponse> =
        Decode.object (fun get -> {
            DisabledReasons = get.Required.Field Property.DisabledReasons Decode.int
        })

    let encoder (v: DisabledAppReasonResponse) =
        Encode.object [
            Property.DisabledReasons, Encode.int v.DisabledReasons
        ]

    let fromDomain (reasons: DisabledAppReason list) = {
        DisabledReasons = DisabledAppReason.toBitfield reasons
    }
