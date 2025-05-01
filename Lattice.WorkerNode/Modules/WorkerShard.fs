namespace Lattice.WorkerNode

open FSharp.Discord.Gateway
open FSharp.Discord.Types
open System.Threading
open System.Threading.Tasks

type WorkerShardMetadata = {
    ClientId: string
}

type WorkerShard = {
    Metadata: WorkerShardMetadata
    Client: IGatewayClient
    Process: Task<GatewayCloseEventCode option>
    CancellationToken: CancellationToken
}

module WorkerShard =
    let requestStop (shard: WorkerShard) =
        task {
            do! shard.Client.DisposeAsync()
        }

// TODO: Refactor basically this whole project once FSharp.Discord.Gateway is functional and use it
