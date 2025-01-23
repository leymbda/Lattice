namespace Lattice.Orchestrator.Application

type IEnv =
    inherit IDiscord
    inherit IPersistence
    inherit ITask

// TODO: This feels wrong - What is the correct approach for handling this env and DI across multiple projects?
