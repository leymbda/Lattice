namespace Lattice.Orchestrator.Application

type IEnv =
    inherit IDiscord
    inherit IEvents
    inherit IPersistence
    inherit ISecrets
