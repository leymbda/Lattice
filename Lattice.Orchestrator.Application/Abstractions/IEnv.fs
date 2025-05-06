namespace Lattice.Orchestrator.Application

type IEnv =
    inherit ICache
    inherit IDiscord
    inherit IPool
    inherit IPersistence
    inherit ISecrets
