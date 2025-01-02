namespace Lattice.Orchestrator.Domain

type Handler =
    | WEBHOOK     of WebhookHandler
    | SERVICE_BUS of ServiceBusHandler

and WebhookHandler = {
    Endpoint:          string
    Ed25519PublicKey:  string
    Ed25519PrivateKey: string
}

and ServiceBusHandler = {
    ConnectionString: string
    QueueName:        string
}
