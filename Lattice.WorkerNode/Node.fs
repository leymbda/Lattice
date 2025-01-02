namespace Lattice.WorkerNode

open Lattice.WorkerNode.Factories
open Lattice.WorkerNode.Managers

type Node (
    serviceBusClientFactory: IServiceBusClientFactory,
    shardFactory: IShardFactory
) =
    let rec loop (shards: Shard list) = task {
        // TODO: Connect to orchestrator service bus to await shards to bid on and instantiate

        // TODO: Connect to gateway request bus to await requests to process gateway send events

        // TODO: Figure out how to determine which specific shard has access to make gateway send event requests (is this even necessary?)

        // TODO: Start shard and store in list

        // TODO: Figure out appropriate way to handle notifying orchestrator when shards released

        return! loop shards
    }

    member _.StartAsync () = task {
        do! loop []
    }
