namespace Lattice.Orchestrator.Infrastructure.Tasks

open Microsoft.Azure.Functions.Worker
open Microsoft.DurableTask

type NodeShutdownOrchestrationProps = {
    Id: string
}

type NodeShutdownOrchestration () =
    [<Function(nameof NodeShutdownOrchestration)>]
    member _.Run (
        [<OrchestrationTrigger>] ctx: TaskOrchestrationContext,
        props: NodeShutdownOrchestrationProps
    ) = task {
        return () 

        // TODO: Implement
        // - Send message to node telling to to begin shutdown, confirmation will determine whether to treat as graceful or forced
        // - If graceful, node will notify orchestrator as shards go offline and they can be automatically re-provisioned (how to handle avoiding down time? Start new connection with a datetime for when to start sending from new node?)
        // - If forced, all shards will be automatically freed and re-provisioned (what should trigger the re-provisioning? likely a necessary question for any upcoming shard-related work)
        // - Above business logic should be handled in application operations, so these can probably be wrapped in activity functions (?)
    }
