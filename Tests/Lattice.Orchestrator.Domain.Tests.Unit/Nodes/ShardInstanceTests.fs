namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type ShardInstanceTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let shardId = ShardId.create "applicationId" 0 1
        let nodeId = Guid.NewGuid()

        // Act
        let shardInstance = ShardInstance.create shardId nodeId

        // Assert
        Assert.AreEqual<ShardId>(shardId, shardInstance.ShardId)
        Assert.AreEqual<Guid>(nodeId, shardInstance.NodeId)
        Assert.AreEqual<DateTime option>(None, shardInstance.StartAt)
        Assert.AreEqual<DateTime option>(None, shardInstance.ShutdownAt)

    [<TestMethod>]
    member _.``start - Sets start date``() =
        // Arrange
        let shardInstance = ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())
        let startAt = DateTime.UtcNow

        // Act
        let updated = ShardInstance.start startAt shardInstance

        // Assert
        Assert.AreEqual<DateTime option>(Some startAt, updated.StartAt)

    [<TestMethod>]
    member _.``Shutdown - Sets end date``() =
        // Arrange
        let shardInstance =
            ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())
            |> ShardInstance.start DateTime.UtcNow

        let shutdownAt = DateTime.UtcNow

        // Act
        let updated = ShardInstance.shutdown shutdownAt shardInstance

        // Assert
        Assert.AreEqual<DateTime option>(Some shutdownAt, updated.ShutdownAt)

    [<TestMethod>]
    member _.``getState - Returns not started state when no start date``() =
        // Arrange
        let shardInstance = ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())

        // Act
        let state = ShardInstance.getState DateTime.UtcNow shardInstance

        // Assert
        match state with
        | ShardInstanceState.NotStarted -> ()
        | _ -> Assert.Fail()

    [<TestMethod>]
    member _.``getState - Returns starting when start date in future and no end date``() =
        // Arrange
        let shardInstance =
            ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())
            |> ShardInstance.start (DateTime.UtcNow.Add (TimeSpan.FromSeconds 30))

        // Act
        let state = ShardInstance.getState DateTime.UtcNow shardInstance

        // Assert
        match state with
        | ShardInstanceState.Starting d -> Assert.IsTrue(d > DateTime.UtcNow)
        | _ -> Assert.Fail()

    [<TestMethod>]
    member _.``getState - Returns active when after start date and no end date``() =
        // Arrange
        let shardInstance =
            ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())
            |> ShardInstance.start (DateTime.UtcNow.Add (TimeSpan.FromSeconds -30))

        // Act
        let state = ShardInstance.getState DateTime.UtcNow shardInstance

        // Assert
        match state with
        | ShardInstanceState.Active -> ()
        | _ -> Assert.Fail()

    [<TestMethod>]
    member _.``getState - Returns shutting down when shutdown date set in future``() =
        // Arrange
        let shardInstance =
            ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())
            |> ShardInstance.start (DateTime.UtcNow.Add (TimeSpan.FromSeconds -30))
            |> ShardInstance.shutdown (DateTime.UtcNow.Add (TimeSpan.FromSeconds 30))

        // Act
        let state = ShardInstance.getState DateTime.UtcNow shardInstance

        // Assert
        match state with
        | ShardInstanceState.ShuttingDown d -> Assert.IsTrue(d > DateTime.UtcNow)
        | _ -> Assert.Fail()

    [<TestMethod>]
    member _.``getState - Returns shutdown when shutdown state in past``() =
        // Arrange
        let shardInstance =
            ShardInstance.create (ShardId.create "applicationId" 0 1) (Guid.NewGuid())
            |> ShardInstance.start (DateTime.UtcNow.Add (TimeSpan.FromSeconds -60))
            |> ShardInstance.shutdown (DateTime.UtcNow.Add (TimeSpan.FromSeconds -30))

        // Act
        let state = ShardInstance.getState DateTime.UtcNow shardInstance

        // Assert
        match state with
        | ShardInstanceState.Shutdown d -> Assert.IsTrue(d < DateTime.UtcNow)
        | _ -> Assert.Fail()
