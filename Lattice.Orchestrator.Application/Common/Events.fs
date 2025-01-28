module Lattice.Orchestrator.Application.Events

let [<Literal>] NODE_HEARTBEAT = "Lattice.Node.Heartbeat"
let [<Literal>] NODE_RELEASE = "Lattice.Node.Release"
let [<Literal>] NODE_REDISTRIBUTE = "Lattice.Node.Redistribute"

let [<Literal>] NODE_HEARTBEAT_FREQUENCY_SECS = 30
