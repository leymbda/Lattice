namespace Lattice.Orchestrator.Contracts

open System
open Thoth.Json.Net

type NodeHeartbeatMessage = {
    HeartbeatTime: DateTime
}

module NodeHeartbeatMessage =
    module Property =
        let [<Literal>] HeartbeatTime = "heartbeatTime"

    let decoder: Decoder<NodeHeartbeatMessage> =
        Decode.object (fun get -> {
            HeartbeatTime = get.Required.Field Property.HeartbeatTime Decode.datetimeUtc
        })

    let encoder (v: NodeHeartbeatMessage) =
        Encode.object [
            Property.HeartbeatTime, Encode.datetime v.HeartbeatTime
        ]
    