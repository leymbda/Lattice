namespace Lattice.Orchestrator.Application

open System
open System.Threading.Tasks

type IEvents =
    abstract NodeHeartbeat: nodeId: Guid -> heartbeatTime: DateTime -> Task
    abstract NodeRelease: nodeId: Guid -> Task
    abstract NodeRedistribute: nodeId: Guid -> Task
