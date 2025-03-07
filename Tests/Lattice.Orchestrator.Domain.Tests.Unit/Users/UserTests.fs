namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type UserTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let id = "id"
        let username = "username"

        // Act
        let user = User.create id username

        // Assert
        Assert.AreEqual<string>(id, user.Id)
        Assert.AreEqual<string>(username, user.Username)

    [<TestMethod>]
    member _.``setUsername - Sets username`` () =
        // Arrange
        let user = User.create "id" "username"
        let newUsername = "newUsername"

        // Act
        let updatedUser = User.setUsername newUsername user

        // Assert
        Assert.AreEqual<string>(newUsername, updatedUser.Username)
