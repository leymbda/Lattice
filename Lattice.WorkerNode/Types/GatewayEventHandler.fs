namespace Lattice.WorkerNode.Types

open System

type GatewayEventHandler =
    | Http of endpoint: Uri
    | ServiceBus of connectionString: string * queueName: string
