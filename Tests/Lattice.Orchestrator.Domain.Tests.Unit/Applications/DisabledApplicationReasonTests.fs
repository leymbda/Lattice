namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting
open System

[<TestClass>]
type DisabledApplicationReasonTests () =
    [<TestMethod>]
    [<DataRow(0)>]
    [<DataRow(1, 1)>]
    [<DataRow(3, 1, 2)>]
    [<DataRow(10, 2, 8)>]
    member _.``toBitfield - Creates bitfield with correct bits flipped`` (
        expected: int,
        [<ParamArray>] ints: int array
    ) =
        // Arrange
        let reasons = ints |> Array.toList |> List.map enum<DisabledApplicationReason>

        // Act
        let bitfield = DisabledApplicationReason.toBitfield reasons

        // Assert
        Assert.AreEqual<int>(expected, bitfield)

    [<TestMethod>]
    [<DataRow(0)>]
    [<DataRow(1, 1)>]
    [<DataRow(3, 1, 2)>]
    [<DataRow(10, 2, 8)>]
    member _.``fromBitfield - Creates list with correct values`` (
        bitfield: int,
        [<ParamArray>] ints: int array
    ) =
        // Arrange
        let expected = ints |> Array.toList |> List.map enum<DisabledApplicationReason>

        // Act
        let reasons = DisabledApplicationReason.fromBitfield bitfield

        // Assert
        Assert.AreEqual<DisabledApplicationReason list>(expected, reasons)
