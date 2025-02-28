namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type BidTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let nodeId = Guid.NewGuid()
        let amount = 1

        // Act
        let bid = Bid.create nodeId amount

        // Assert
        Assert.AreEqual<Guid>(nodeId, bid.NodeId)
        Assert.AreEqual<int>(amount, bid.Amount)
