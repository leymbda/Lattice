module Lattice.Orchestrator.Infrastructure.Persistence.HandlerMapper

open Lattice.Orchestrator.Domain

let toDomain model =
    match model with
    | HandlerModel.WEBHOOK model ->
        Handler.WEBHOOK {
            Endpoint = model.Endpoint
            Ed25519PublicKey = model.Ed25519PublicKey
            Ed25519PrivateKey = model.Ed25519PrivateKey
        }

    | HandlerModel.SERVICE_BUS model ->
        Handler.SERVICE_BUS {
            ConnectionString = model.ConnectionString
            QueueName = model.QueueName
        }

let fromDomain handler =
    match handler with
    | Handler.WEBHOOK handler ->
        HandlerModel.WEBHOOK {
            Type = HandlerModelType.WEBHOOK
            Endpoint = handler.Endpoint
            Ed25519PublicKey = handler.Ed25519PublicKey
            Ed25519PrivateKey = handler.Ed25519PrivateKey
        }

    | Handler.SERVICE_BUS handler ->
        HandlerModel.SERVICE_BUS {
            Type = HandlerModelType.SERVICE_BUS
            ConnectionString = handler.ConnectionString
            QueueName = handler.QueueName
        }
