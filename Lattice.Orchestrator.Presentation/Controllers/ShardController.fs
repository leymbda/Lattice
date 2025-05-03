namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http

type ShardController (env: IEnv) =
    [<Function "ListShards">]
    member _.ListShards (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apps/{appId:long}/shards")>] req: HttpRequestData,
        appId: int64
    ) = task {
        raise (System.NotImplementedException())
    }
    
    [<Function "ListShardInstances">]
    member _.GetShard (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apps/{appId:long}/shard-instances")>] req: HttpRequestData,
        appId: int64
    ) = task {
        raise (System.NotImplementedException())
    }

    // TODO: Implement above
    // TODO: Endpoints for shards in nodes?
