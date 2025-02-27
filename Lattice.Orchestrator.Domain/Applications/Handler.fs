namespace rec Lattice.Orchestrator.Domain

type Handler =
    | WEBHOOK     of WebhookHandler
    | SERVICE_BUS of ServiceBusHandler

type WebhookHandler = {
    Endpoint:          string
    Ed25519PublicKey:  string
    Ed25519PrivateKey: string
}

type ServiceBusHandler = {
    ConnectionString: string
    QueueName:        string
}

module WebhookHandler =
    let create endpoint ed25519PublicKey ed25519PrivateKey =
        {
            Endpoint = endpoint
            Ed25519PublicKey = ed25519PublicKey
            Ed25519PrivateKey = ed25519PrivateKey
        }

module ServiceBusHandler =
    let create connectionString queueName =
        {
            ConnectionString = connectionString
            QueueName = queueName
        }
