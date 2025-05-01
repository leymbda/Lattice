﻿namespace Lattice.WorkerNode

open Azure.Messaging.ServiceBus
open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain
open System.Net.Http
open System.Net.Http.Headers
open System.Threading

type Node (
    serviceBusClientFactory: IServiceBusClientFactory,
    httpClientFactory: IHttpClientFactory,
    gatewayClientFactory: IGatewayClientFactory
) =
    member val private _shards: WorkerShard list = [] with get, set

    member this.AddShard gatewayUrl clientId identify handler = task {
        let handler =
            match handler with
            | Handler.WEBHOOK handler ->
                let client = httpClientFactory.CreateClient()

                fun (json: string) -> task {
                    let req = new HttpRequestMessage(HttpMethod.Post, handler.Endpoint)
                    req.Content <- new StringContent(json, MediaTypeHeaderValue("application/json"))
                        
                    // TODO: Create ed25519 signature like Discord to ensure only valid events are accepted downstream

                    let! res = client.SendAsync req

                    // TODO: Handle status code

                    return ()
                }

            | Handler.SERVICE_BUS handler ->
                let client = serviceBusClientFactory.CreateClient handler.ConnectionString
                let sender = client.CreateSender handler.QueueName

                fun (json: string) -> task {
                    do! sender.SendMessageAsync <| ServiceBusMessage json

                    // TODO: Handle exceptions for if sending fails
                }

        use cts = new CancellationTokenSource()
        let ct = cts.Token

        let metadata = { ClientId = clientId }
        let client = gatewayClientFactory.CreateClient()
        let proc = client.Connect gatewayUrl identify handler ct

        let shard = {
            Metadata = metadata
            Client = client
            Process = proc
            CancellationToken = ct
        }

        this._shards <- shard :: this._shards
        return shard
    }

    // TODO: Does setting up the node this way even make sense?

    member _.StartAsync () = task {
        // TODO: Connect to orchestrator service bus to await shards to bid on and instantiate

        // TODO: Connect to gateway request bus to await requests to process gateway send events

        // TODO: Figure out how to determine which specific shard has access to make gateway send event requests (is this even necessary?)

        // TODO: Start shard and store in list

        // TODO: Figure out appropriate way to handle notifying orchestrator when shards released

        return ()
    }
