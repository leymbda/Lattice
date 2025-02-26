namespace Lattice.Orchestrator.Presentation

open Thoth.Json.Net

type SetServiceBusApplicationHandlerPayload = {
    ConnectionString: string
    QueueName: string
}

module SetServiceBusApplicationHandlerPayload =
    let decoder: Decoder<SetServiceBusApplicationHandlerPayload> =
        Decode.object (fun get -> {
            ConnectionString = get.Required.Field "connectionString" Decode.string
            QueueName = get.Required.Field "queueName" Decode.string
        })
