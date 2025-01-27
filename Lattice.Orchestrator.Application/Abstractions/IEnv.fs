namespace Lattice.Orchestrator.Application

type IEnv =
    inherit IDiscord
    inherit IPersistence
    inherit INodeEntityClient

// TODO: This feels wrong - What is the correct approach for handling this env and DI across multiple projects?
