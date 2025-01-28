namespace Lattice.Orchestrator.Application

open System
open System.Threading.Tasks

type INodeEntityClient =
    abstract Heartbeat: nodeId: Guid -> heartbeatTime: DateTime -> Task
    abstract Release: nodeId: Guid -> Task
    abstract Redistribute: nodeId: Guid -> Task
