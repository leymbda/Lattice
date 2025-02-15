namespace Lattice.WorkerNode

open FSharp.Discord.Gateway
open FSharp.Discord.Types
open System.Threading
open System.Threading.Tasks

type ShardMetadata = {
    ClientId: string
}

type Shard = {
    Metadata: ShardMetadata
    Client: IGatewayClient
    Process: Task<GatewayCloseEventCode option>
    CancellationToken: CancellationToken
}

module Shard =
    let requestStop (shard: Shard) =
        task {
            do! shard.Client.DisposeAsync()
        }
