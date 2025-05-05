namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type ShardIdTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let applicationId = "applicationId"
        let formulaId = 0
        let numShards = 1

        // Act
        let shardId = ShardId.create applicationId formulaId numShards

        // Assert
        match shardId with
        | ShardId (actualApplicationId, actualFormulaId, actualNumShards) ->
            Assert.AreEqual<string>(applicationId, actualApplicationId)
            Assert.AreEqual<int>(formulaId, actualFormulaId)
            Assert.AreEqual<int>(numShards, actualNumShards)

        // TODO: I think there's a nicer way to grab these values from the single DU
        
    [<TestMethod>]
    member _.``toString - Converts to expected string`` () =
        // Arrange
        let shardId = ShardId.create "applicationId" 0 1
        let expected = "applicationId:0:1"

        // Act
        let actual = ShardId.toString shardId

        // Assert
        Assert.AreEqual<string>(expected, actual)
        
    [<TestMethod>]
    [<DataRow "">]
    [<DataRow "invalid">]
    [<DataRow "applicationId:notanint:1">]
    [<DataRow "applicationId:0:notanint">]
    member _.``fromString - Returns none when given invalid string`` (str: string) =
        // Arrange
        let expected = ShardId.create "applicationId" 0 1

        // Act
        let actual = ShardId.fromString str

        // Assert
        match actual with
        | None -> ()
        | _ -> Assert.Fail()
        
    [<TestMethod>]
    member _.``fromString - Converts valid string into shard id`` () =
        // Arrange
        let str = "applicationId:0:1"
        let expected = ShardId.create "applicationId" 0 1

        // Act
        let actual = ShardId.fromString str

        // Assert
        match actual with
        | Some actual -> Assert.AreEqual<ShardId>(expected, actual)
        | _ -> Assert.Fail()

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

    [<TestMethod>]
    member _.``addInstance - Adds node to start of empty list`` () =
        // Arrange
        let shard = Shard.create "applicationId" 0 1

        let nodeId = Guid.NewGuid()
        let createAt = DateTime.UtcNow

        // Act
        let updated = Shard.addInstance nodeId createAt shard

        // Assert
        Assert.AreEqual<DateTime * Guid option>((createAt, Some nodeId), updated.Instances |> List.head)
        Assert.AreEqual<int>(1, updated.Instances |> List.length)

    [<TestMethod>]
    member _.``addInstance - Adds node to start of list with existing instances`` () =
        // Arrange
        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance (Guid.NewGuid()) DateTime.UtcNow
            |> Shard.addInstance (Guid.NewGuid()) DateTime.UtcNow

        let nodeId = Guid.NewGuid()
        let createAt = DateTime.UtcNow

        // Act
        let updated = Shard.addInstance nodeId createAt shard

        // Assert
        Assert.AreEqual<DateTime * Guid option>((createAt, Some nodeId), updated.Instances |> List.head)
        Assert.AreEqual<int>(3, updated.Instances |> List.length)

    [<TestMethod>]
    member _.``shutdown - Sets empty node to start of instances list to indicate shutdown`` () =
        // Arrange
        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance (Guid.NewGuid()) DateTime.UtcNow
            |> Shard.addInstance (Guid.NewGuid()) DateTime.UtcNow
            
        let shutdownAt = DateTime.UtcNow

        // Act
        let updated = Shard.shutdown shutdownAt shard

        // Assert
        Assert.AreEqual<DateTime * Guid option>((shutdownAt, None), updated.Instances |> List.head)
        Assert.AreEqual<int>(3, updated.Instances |> List.length)
        
    [<TestMethod>]
    member _.``getState - Returns not started if no instances`` () =
        // Arrange
        let currentTime = DateTime.UtcNow

        let shard = Shard.create "applicationId" 0 1

        // Act
        let state = Shard.getState currentTime shard

        // Assert
        match state with
        | ShardState.NotStarted -> ()
        | _ -> Assert.Fail()
        
    [<TestMethod>]
    member _.``getState - Returns active when latest instance is a node that has started`` () =
        // Arrange
        let currentTime = DateTime.UtcNow

        let nodeId = Guid.NewGuid()
        let createAt = currentTime.AddSeconds -30

        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance nodeId createAt

        // Act
        let state = Shard.getState currentTime shard

        // Assert
        match state with
        | ShardState.Active current -> Assert.AreEqual<Guid>(nodeId, current)
        | _ -> Assert.Fail()
        
    [<TestMethod>]
    member _.``getState - Returns transferring when latest instance not started node and previous instance is a node`` () =
        // Arrange
        let currentTime = DateTime.UtcNow

        let currentNodeId = Guid.NewGuid()
        let currentCreateAt = currentTime.AddSeconds -30
        let nextNodeId = Guid.NewGuid()
        let nextCreateAt = currentTime.AddSeconds 30

        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance currentNodeId currentCreateAt
            |> Shard.addInstance nextNodeId nextCreateAt

        // Act
        let state = Shard.getState currentTime shard

        // Assert
        match state with
        | ShardState.Transferring (current, next, transferAt) ->
            Assert.AreEqual<Guid>(currentNodeId, current)
            Assert.AreEqual<Guid>(nextNodeId, next)
            Assert.AreEqual<DateTime>(nextCreateAt, transferAt)
        | _ -> Assert.Fail()
        
    [<TestMethod>]
    member _.``getState - Returns starting when latest instance is a node that hasn't started`` () =
        // Arrange
        let currentTime = DateTime.UtcNow

        let nodeId = Guid.NewGuid()
        let createAt = currentTime.AddSeconds 30

        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance nodeId createAt

        // Act
        let state = Shard.getState currentTime shard

        // Assert
        match state with
        | ShardState.Starting (next, time) ->
            Assert.AreEqual<Guid>(nodeId, next)
            Assert.AreEqual<DateTime>(createAt, time)
        | _ -> Assert.Fail()
        
    [<TestMethod>]
    member _.``getState - Returns shutting down when latest instance none and not yet shut down and previous instance is a node`` () =
        // Arrange
        let currentTime = DateTime.UtcNow

        let nodeId = Guid.NewGuid()
        let createAt = currentTime.AddSeconds -30
        let shutdownAtTime = currentTime.AddSeconds 30

        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance nodeId createAt
            |> Shard.shutdown shutdownAtTime

        // Act
        let state = Shard.getState currentTime shard

        // Assert
        match state with
        | ShardState.ShuttingDown (current, shutdownAt) ->
            Assert.AreEqual<Guid>(nodeId, current)
            Assert.AreEqual<DateTime>(shutdownAtTime, shutdownAt)
        | _ -> Assert.Fail()
        
    [<TestMethod>]
    member _.``getState - Returns shutdown when latest instance is none`` () =
        // Arrange
        let currentTime = DateTime.UtcNow

        let nodeId = Guid.NewGuid()
        let createAt = currentTime.AddSeconds -60
        let shutdownAtTime = currentTime.AddSeconds -30

        let shard =
            Shard.create "applicationId" 0 1
            |> Shard.addInstance nodeId createAt
            |> Shard.shutdown shutdownAtTime

        // Act
        let state = Shard.getState currentTime shard

        // Assert
        match state with
        | ShardState.Shutdown shutdownAt -> Assert.AreEqual<DateTime>(shutdownAtTime, shutdownAt)
        | _ -> Assert.Fail()
