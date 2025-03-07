namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

// TODO: Create ShardIdTests

[<TestClass>]
type ShardTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let applicationId = "applicationId"
        let formulaId = 0
        let numShards = 1

        // Act
        let shard = Shard.create applicationId formulaId numShards

        // Assert
        Assert.AreEqual<ShardId>(ShardId (applicationId, formulaId, numShards), shard.Id)
        Assert.IsTrue(shard.Instances |> List.isEmpty)

    // TODO: Add remaining tests for watching shard state
