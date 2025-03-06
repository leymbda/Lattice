namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type NodeTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let id = Guid.NewGuid()
        let currentTime = DateTime.UtcNow

        // Act
        let node = Node.create id currentTime

        // Assert
        Assert.AreEqual<Guid>(id, node.Id)
        Assert.AreEqual<int>(0, List.length node.Shards)
        Assert.AreEqual<DateTime>(currentTime, node.LastHeartbeatAck)

    [<TestMethod>]
    member _.``addShard - Adds first shard to node`` () =
        // Arrange
        let node = Node.create (Guid.NewGuid()) DateTime.UtcNow
        let shardId = ShardId.create "" 0 1

        // Act
        let updatedNode = Node.addShard shardId node

        // Assert
        Assert.AreEqual<int>(1, List.length updatedNode.Shards)
        Assert.AreEqual<ShardId>(shardId, List.head updatedNode.Shards)

    [<TestMethod>]
    member _.``addShard - Adds shard to node with existing shards`` () =
        // Arrange
        let node =
            Node.create (Guid.NewGuid()) DateTime.UtcNow
            |> Node.addShard (ShardId.create "" 0 3)
            |> Node.addShard (ShardId.create "" 1 3)

        let shardId = ShardId.create "" 2 3

        // Act
        let updatedNode = Node.addShard shardId node

        // Assert
        Assert.IsTrue(List.exists ((=) shardId) updatedNode.Shards)
        Assert.AreEqual<int>(3, List.length updatedNode.Shards)

    [<TestMethod>]
    member _.``removeShard - Removes shard from node`` () =
        // Arrange
        let shardId = ShardId.create "" 0 1
        let node =
            Node.create (Guid.NewGuid()) DateTime.UtcNow
            |> Node.addShard shardId

        // Act
        let updatedNode = Node.removeShard shardId node

        // Assert
        Assert.IsFalse(List.exists ((=) shardId) updatedNode.Shards)
        Assert.AreEqual<int>(0, List.length updatedNode.Shards)

    [<TestMethod>]
    member _.``removeShard - Removes shard from node with multiple shards`` () =
        // Arrange
        let shardId = ShardId.create "" 0 3
        let node =
            Node.create (Guid.NewGuid()) DateTime.UtcNow
            |> Node.addShard shardId
            |> Node.addShard (ShardId.create "" 1 3)
            |> Node.addShard (ShardId.create "" 2 3)

        // Act
        let updatedNode = Node.removeShard shardId node

        // Assert
        Assert.IsFalse(List.exists ((=) shardId) updatedNode.Shards)
        Assert.AreEqual<int>(2, List.length updatedNode.Shards)

    [<TestMethod>]
    member _.``heartbeat - Updates last heartbeat ack`` () =
        // Arrange
        let currentTime = DateTime.UtcNow
        let previousAckTime = currentTime.Subtract (TimeSpan.FromSeconds 10)
        let node = Node.create (Guid.NewGuid()) previousAckTime

        // Act
        let updatedNode = Node.heartbeat currentTime node

        // Assert
        Assert.AreEqual<DateTime>(currentTime, updatedNode.LastHeartbeatAck)
        
    [<TestMethod>]
    member _.``getState - Returns expired state when heartbeat too old`` () =
        // Arrange
        let expiredTime = DateTime.UtcNow.Add (TimeSpan.FromSeconds -Node.LIFETIME_SECONDS)
        let node = Node.create (Guid.NewGuid()) expiredTime

        // Act
        let state = Node.getState DateTime.UtcNow node

        // Assert
        match state with
        | NodeState.Expired -> ()
        | _ -> Assert.Fail()

    [<TestMethod>]
    member _.``getState - Returns active state when heartbeat recent`` () =
        // Arrange
        let currentTime = DateTime.UtcNow
        let node = Node.create (Guid.NewGuid()) currentTime

        // Act
        let state = Node.getState currentTime node

        // Assert
        match state with
        | NodeState.Active -> ()
        | _ -> Assert.Fail()
