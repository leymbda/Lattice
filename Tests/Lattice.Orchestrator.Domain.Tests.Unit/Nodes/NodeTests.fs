namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type NodeTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let id = Guid.NewGuid()

        // Act
        let node = Node.create id

        // Assert
        Assert.AreEqual<Guid>(id, node.Id)
        Assert.AreEqual<int>(0, List.length node.Shards)

    [<TestMethod>]
    member _.``addShard - Adds first shard to node`` () =
        // Arrange
        let node = Node.create (Guid.NewGuid())
        let shardId = Guid.NewGuid()

        // Act
        let updatedNode = Node.addShard shardId node

        // Assert
        Assert.AreEqual<int>(1, List.length updatedNode.Shards)
        Assert.AreEqual<Guid>(shardId, List.head updatedNode.Shards)

    [<TestMethod>]
    member _.``addShard - Adds shard to node with existing shards`` () =
        // Arrange
        let node =
            Node.create (Guid.NewGuid())
            |> Node.addShard (Guid.NewGuid())
            |> Node.addShard (Guid.NewGuid())

        let shardId = Guid.NewGuid()

        // Act
        let updatedNode = Node.addShard shardId node

        // Assert
        Assert.IsTrue(List.exists ((=) shardId) updatedNode.Shards)
        Assert.AreEqual<int>(3, List.length updatedNode.Shards)

    [<TestMethod>]
    member _.``removeShard - Removes shard from node`` () =
        // Arrange
        let shardId = Guid.NewGuid()
        let node =
            Node.create (Guid.NewGuid())
            |> Node.addShard shardId

        // Act
        let updatedNode = Node.removeShard shardId node

        // Assert
        Assert.IsFalse(List.exists ((=) shardId) updatedNode.Shards)
        Assert.AreEqual<int>(0, List.length updatedNode.Shards)

    [<TestMethod>]
    member _.``removeShard - Removes shard from node with multiple shards`` () =
        // Arrange
        let shardId = Guid.NewGuid()
        let node =
            Node.create (Guid.NewGuid())
            |> Node.addShard shardId
            |> Node.addShard (Guid.NewGuid())
            |> Node.addShard (Guid.NewGuid())

        // Act
        let updatedNode = Node.removeShard shardId node

        // Assert
        Assert.IsFalse(List.exists ((=) shardId) updatedNode.Shards)
        Assert.AreEqual<int>(2, List.length updatedNode.Shards)
