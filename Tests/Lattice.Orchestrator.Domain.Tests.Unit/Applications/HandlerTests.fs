namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type WebhookHandlerTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let endpoint = "https://example.com"
        let ed25519PublicKey = "ed25519PublicKey"
        let ed25519PrivateKey = "ed25519PrivateKey"

        // Act
        let handler = WebhookHandler.create endpoint ed25519PublicKey ed25519PrivateKey

        // Assert
        Assert.AreEqual<string>(endpoint, handler.Endpoint)
        Assert.AreEqual<string>(ed25519PublicKey, handler.Ed25519PublicKey)
        Assert.AreEqual<string>(ed25519PrivateKey, handler.Ed25519PrivateKey)
        
[<TestClass>]
type ServiceBusHandlerTests () =
    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let connectionString = "connectionString"
        let queueName = "queueName"

        // Act
        let handler = ServiceBusHandler.create connectionString queueName

        // Assert
        Assert.AreEqual<string>(connectionString, handler.ConnectionString)
        Assert.AreEqual<string>(queueName, handler.QueueName)
