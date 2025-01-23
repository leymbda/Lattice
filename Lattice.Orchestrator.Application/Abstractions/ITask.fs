namespace Lattice.Orchestrator.Application

open System.Threading.Tasks

type ITask =
    abstract BeginNodeShutdownTask: id: string -> Task<unit>
