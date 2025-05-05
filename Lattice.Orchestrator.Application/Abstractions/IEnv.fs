namespace Lattice.Orchestrator.Application

type IEnv =
    inherit ICache
    inherit IDiscord
    inherit IEvents
    inherit IPersistence
    inherit ISecrets
