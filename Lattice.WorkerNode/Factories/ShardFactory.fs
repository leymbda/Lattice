namespace Lattice.WorkerNode.Factories

open Azure.Messaging.ServiceBus
open FSharp.Discord.Gateway.Types
open Lattice.WorkerNode.Managers
open Lattice.WorkerNode.Types
open System.Net.Http
open System.Net.Http.Headers
open System.Threading.Tasks

type IShardFactory =
    abstract member CreateShard:
        gatewayUrl: string ->
        identify: IdentifySendEvent ->
        eventHandler: GatewayEventHandler ->
        Shard
        
type ShardFactory (
    serviceBusClientFactory: IServiceBusClientFactory,
    httpClientFactory: IHttpClientFactory
) =
    interface IShardFactory with
        member _.CreateShard gatewayUrl identify eventHandler =
            let handler =
                match eventHandler with
                | GatewayEventHandler.Http endpoint ->
                    let client = httpClientFactory.CreateClient()

                    fun (json: string) -> task {
                        let req = new HttpRequestMessage(HttpMethod.Post, endpoint)
                        req.Content <- new StringContent(json, MediaTypeHeaderValue("application/json"))
                        
                        // TODO: Create ed25519 signature like Discord to ensure only valid events are accepted downstream

                        do! client.SendAsync req :> Task
                    }

                | GatewayEventHandler.ServiceBus (connectionString, queueName) ->
                    let client = serviceBusClientFactory.CreateClient connectionString
                    let sender = client.CreateSender queueName

                    fun (json: string) -> task {
                        do! sender.SendMessageAsync <| ServiceBusMessage json
                    }

            Shard(gatewayUrl, identify, handler)
