namespace Lattice.Orchestrator.Infrastructure.Pool

open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type Temp () =
    [<TestMethod>]
    member _.``Temporary test to ensure MSTest runner success`` () =
        Assert.IsTrue(true)
