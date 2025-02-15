namespace Lattice.WorkerNode

open Azure.Messaging.ServiceBus

type IServiceBusClientFactory =
    abstract member CreateClient:
        connectionString: string ->
        ServiceBusClient

type ServiceBusClientFactory () =
    interface IServiceBusClientFactory with
        member _.CreateClient (connectionString: string) =
            ServiceBusClient(
                connectionString,
                ServiceBusClientOptions(TransportType = ServiceBusTransportType.AmqpWebSockets)
            )
