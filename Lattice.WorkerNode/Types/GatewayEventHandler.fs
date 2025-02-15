namespace Lattice.WorkerNode

open System

type GatewayEventHandler =
    | Http of endpoint: Uri * ed25519PrivateKey: string
    | ServiceBus of queueName: string * connectionString: string
