namespace Lattice.Orchestrator.Presentation

open System.Text.Json.Serialization

type SetServiceBusApplicationHandlerPayload (connectionString, queueName) =
    [<JsonPropertyName "connectionString">]
    member _.ConnectionString: string = connectionString

    [<JsonPropertyName "queueName">]
    member _.QueueName: string = queueName
