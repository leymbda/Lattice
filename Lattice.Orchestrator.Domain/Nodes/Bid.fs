namespace Lattice.Orchestrator.Domain

type Bid = {
    NodeId: string
    Amount: int
}

module Bid =
    let create nodeId amount =
        {
            NodeId = nodeId
            Amount = amount
        }
