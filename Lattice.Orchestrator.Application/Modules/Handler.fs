namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

module WebhookHandler =
    let create endpoint ed25519PublicKey ed25519PrivateKey = {
        Endpoint = endpoint
        Ed25519PublicKey = ed25519PublicKey
        Ed25519PrivateKey = ed25519PrivateKey
    }

module ServiceBusHandler =
    let create connectionString queueName = {
        ConnectionString = connectionString
        QueueName = queueName
    }
