namespace Lattice.Orchestrator.Domain

open System

type Bid = {
    NodeId: Guid
    Amount: int
}

module Bid =
    let create nodeId amount =
        {
            NodeId = nodeId
            Amount = amount
        }
