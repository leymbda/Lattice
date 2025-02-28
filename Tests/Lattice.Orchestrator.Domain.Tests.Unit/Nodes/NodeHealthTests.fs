namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type NodeHealthTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let id = Guid.NewGuid()
        let currentTime = DateTime.UtcNow

        // Act
        let nodeHealth = NodeHealth.create id currentTime

        // Assert
        Assert.AreEqual<Guid>(id, nodeHealth.Id)
        Assert.AreEqual<DateTime>(currentTime, nodeHealth.LastHeartbeatAck)
        Assert.AreEqual<DateTime option>(None, nodeHealth.ScheduledCutoff)
        Assert.IsFalse(nodeHealth.TransferReady)

    [<TestMethod>]
    member _.``isAlive - Returns true when node is alive`` () =
        // Arrange
        let currentTime = DateTime.UtcNow
        let nodeHealth = NodeHealth.create (Guid.NewGuid()) currentTime

        // Act
        let result = NodeHealth.isAlive currentTime nodeHealth

        // Assert
        Assert.IsTrue(result)

    [<TestMethod>]
    member _.``isAlive - Returns false when node is not alive`` () =
        // Arrange
        let currentTime = DateTime.UtcNow
        let timeBeforeLifetime = currentTime.Subtract (TimeSpan.FromSeconds (NodeHealth.LIFETIME_SECONDS + 1 |> float))
        let nodeHealth = NodeHealth.create (Guid.NewGuid()) timeBeforeLifetime

        // Act
        let result = NodeHealth.isAlive currentTime nodeHealth

        // Assert
        Assert.IsFalse(result)

    [<TestMethod>]
    member _.``heartbeat - Updates last heartbeat ack`` () =
        // Arrange
        let currentTime = DateTime.UtcNow
        let previousAckTime = currentTime.Subtract (TimeSpan.FromSeconds 10)
        let nodeHealth = NodeHealth.create (Guid.NewGuid()) previousAckTime

        // Act
        let updatedNodeHealth = NodeHealth.heartbeat currentTime nodeHealth

        // Assert
        Assert.AreEqual<DateTime>(currentTime, updatedNodeHealth.LastHeartbeatAck)

    [<TestMethod>]
    member _.``initiateRedistribution - Sets the scheduled cutoff`` () =
        // Arrange
        let currentTime = DateTime.UtcNow
        let nodeHealth = NodeHealth.create (Guid.NewGuid()) currentTime

        // Act
        let updatedNodeHealth = NodeHealth.initiateRedistribution currentTime nodeHealth

        // Assert
        match updatedNodeHealth.ScheduledCutoff with
        | None -> Assert.Fail()
        | Some cutoff -> Assert.AreEqual<DateTime>(currentTime.AddSeconds NodeHealth.REDISTRIBUTION_CUTOFF_SECONDS, cutoff)

    // TODO: This domain model is incomplete, and will require more tests as it is implemented
