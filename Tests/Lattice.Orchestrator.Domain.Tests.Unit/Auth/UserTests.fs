namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type UserTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let id = "id"
        let username = "username"
        let encryptedAccessToken = "encryptedAccessToken"
        let encryptedRefreshToken = "encryptedRefreshToken"

        // Act
        let user = User.create id username encryptedAccessToken encryptedRefreshToken

        // Assert
        Assert.AreEqual<string>(id, user.Id)
        Assert.AreEqual<string>(username, user.Username)
        Assert.AreEqual<string>(encryptedAccessToken, user.EncryptedAccessToken)
        Assert.AreEqual<string>(encryptedRefreshToken, user.EncryptedRefreshToken)

    [<TestMethod>]
    member _.``setUsername - Sets username`` () =
        // Arrange
        let user = User.create "id" "username" "encryptedAccessToken" "encryptedRefreshToken"
        let newUsername = "newUsername"

        // Act
        let updatedUser = User.setUsername newUsername user

        // Assert
        Assert.AreEqual<string>(newUsername, updatedUser.Username)

    [<TestMethod>]
    member _.``setEncryptedAccessToken - Sets encrypted access token`` () =
        // Arrange
        let user = User.create "id" "username" "encryptedAccessToken" "encryptedRefreshToken"
        let newEncryptedAccessToken = "newEncryptedAccessToken"

        // Act
        let updatedUser = User.setEncryptedAccessToken newEncryptedAccessToken user

        // Assert
        Assert.AreEqual<string>(newEncryptedAccessToken, updatedUser.EncryptedAccessToken)

    [<TestMethod>]
    member _.``setEncryptedRefreshToken - Sets encrypted refresh token`` () =
        // Arrange
        let user = User.create "id" "username" "encryptedAccessToken" "encryptedRefreshToken"
        let newEncryptedRefreshToken = "newEncryptedRefreshToken"

        // Act
        let updatedUser = User.setEncryptedRefreshToken newEncryptedRefreshToken user

        // Assert
        Assert.AreEqual<string>(newEncryptedRefreshToken, updatedUser.EncryptedRefreshToken)
