namespace Lattice.Orchestrator.Application

open System
open System.Threading.Tasks

type INodeEntityClient =
    abstract Heartbeat: nodeId: Guid -> Task
