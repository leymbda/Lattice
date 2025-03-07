namespace Lattice.Orchestrator.AppHost

open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type Temp () =
    [<TestMethod>]
    member _.``Temporary test to ensure MSTest runner success`` () =
        Assert.IsTrue(true)
