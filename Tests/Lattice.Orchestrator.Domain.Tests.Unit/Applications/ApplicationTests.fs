namespace Lattice.Orchestrator.Domain

open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type ApplicationTests () =
    let _defaultPrivilegedIntents = {
        MessageContent = false
        MessageContentLimited = false
        GuildMembers = false
        GuildMembersLimited = false
        Presence = false
        PresenceLimited = false
    }

    [<TestMethod>]
    member _.``create - Creates with provided values`` () =
        // Arrange
        let id = "id"
        let encryptedBotToken = "encryptedBotToken"
        let privilegedIntents = _defaultPrivilegedIntents

        // Act
        let app = Application.create id encryptedBotToken privilegedIntents

        // Assert
        Assert.AreEqual<string>(id, app.Id)
        Assert.AreEqual<string>(encryptedBotToken, app.EncryptedBotToken)
        Assert.AreEqual<bool>(privilegedIntents.MessageContent, app.PrivilegedIntents.MessageContent)
        Assert.AreEqual<bool>(privilegedIntents.MessageContentLimited, app.PrivilegedIntents.MessageContentLimited)
        Assert.AreEqual<bool>(privilegedIntents.GuildMembers, app.PrivilegedIntents.GuildMembers)
        Assert.AreEqual<bool>(privilegedIntents.GuildMembersLimited, app.PrivilegedIntents.GuildMembersLimited)
        Assert.AreEqual<bool>(privilegedIntents.Presence, app.PrivilegedIntents.Presence)
        Assert.AreEqual<bool>(privilegedIntents.PresenceLimited, app.PrivilegedIntents.PresenceLimited)

    [<TestMethod>]
    member _.``setEncryptedBotToken - Sets encrypted bot token`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
        let newEncryptedBotToken = "newEncryptedBotToken"

        // Act
        let newApp = Application.setEncryptedBotToken newEncryptedBotToken app

        // Assert
        Assert.AreEqual<string>(newEncryptedBotToken, newApp.EncryptedBotToken)

    [<TestMethod>]
    member _.``setPrivilegedIntents - Sets privileged intents`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
        let newPrivilegedIntents = {
            MessageContent = true
            MessageContentLimited = true
            GuildMembers = true
            GuildMembersLimited = true
            Presence = true
            PresenceLimited = true
        }

        // Act
        let newApp = Application.setPrivilegedIntents newPrivilegedIntents app

        // Assert
        Assert.AreEqual<bool>(newPrivilegedIntents.MessageContent, newApp.PrivilegedIntents.MessageContent)
        Assert.AreEqual<bool>(newPrivilegedIntents.MessageContentLimited, newApp.PrivilegedIntents.MessageContentLimited)
        Assert.AreEqual<bool>(newPrivilegedIntents.GuildMembers, newApp.PrivilegedIntents.GuildMembers)
        Assert.AreEqual<bool>(newPrivilegedIntents.GuildMembersLimited, newApp.PrivilegedIntents.GuildMembersLimited)
        Assert.AreEqual<bool>(newPrivilegedIntents.Presence, newApp.PrivilegedIntents.Presence)
        Assert.AreEqual<bool>(newPrivilegedIntents.PresenceLimited, newApp.PrivilegedIntents.PresenceLimited)

    [<TestMethod>]
    member _.``addDisabledReason - Adds new disabled reason`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents

        // Act
        let newApp = Application.addDisabledReason DisabledApplicationReason.BLACKLISTED app

        // Assert
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))
        Assert.AreEqual<int>(1, newApp.DisabledReasons |> List.filter (fun r -> r = DisabledApplicationReason.BLACKLISTED) |> List.length)

    [<TestMethod>]
    member _.``addDisabledReason - Doesn't duplicate existing reason`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.addDisabledReason DisabledApplicationReason.BLACKLISTED

        // Act
        let newApp = Application.addDisabledReason DisabledApplicationReason.BLACKLISTED app

        // Assert
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))
        Assert.AreEqual<int>(1, newApp.DisabledReasons |> List.filter (fun r -> r = DisabledApplicationReason.BLACKLISTED) |> List.length)
        
    [<TestMethod>]
    member _.``addDisabledReason - Adds to existing set of reasons`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.addDisabledReason DisabledApplicationReason.NOT_ENOUGH_SHARDS

        // Act
        let newApp = Application.addDisabledReason DisabledApplicationReason.BLACKLISTED app

        // Assert
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.NOT_ENOUGH_SHARDS))
        Assert.AreEqual<int>(2, newApp.DisabledReasons |> List.length)

    [<TestMethod>]
    member _.``removeDisabledReason - Removes existing reason`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.addDisabledReason DisabledApplicationReason.BLACKLISTED

        // Act
        let newApp = Application.removeDisabledReason DisabledApplicationReason.BLACKLISTED app

        // Assert
        Assert.IsFalse(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))

    [<TestMethod>]
    member _.``removeDisabledReason - Doesn't remove non-existing reason`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents

        // Act
        let newApp = Application.removeDisabledReason DisabledApplicationReason.BLACKLISTED app

        // Assert
        Assert.IsFalse(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))

    [<TestMethod>]
    member _.``removeDisabledReason - Removes one of several reasons`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.addDisabledReason DisabledApplicationReason.BLACKLISTED
            |> Application.addDisabledReason DisabledApplicationReason.NOT_ENOUGH_SHARDS

        // Act
        let newApp = Application.removeDisabledReason DisabledApplicationReason.BLACKLISTED app

        // Assert
        Assert.IsFalse(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.NOT_ENOUGH_SHARDS))
        Assert.AreEqual<int>(1, newApp.DisabledReasons |> List.length)

    [<TestMethod>]
    member _.``setDisabledReasons - Sets disabled reasons from none`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents

        // Act
        let newApp = Application.setDisabledReasons [DisabledApplicationReason.BLACKLISTED; DisabledApplicationReason.NOT_ENOUGH_SHARDS] app

        // Assert
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.NOT_ENOUGH_SHARDS))
        Assert.AreEqual<int>(2, newApp.DisabledReasons |> List.length)

    [<TestMethod>]
    member _.``setDisabledReasons - Overwrites previous disabled reasons`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.addDisabledReason DisabledApplicationReason.BLACKLISTED

        // Act
        let newApp = Application.setDisabledReasons [DisabledApplicationReason.NOT_ENOUGH_SHARDS] app

        // Assert
        Assert.IsFalse(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.BLACKLISTED))
        Assert.IsTrue(newApp.DisabledReasons |> List.exists (fun r -> r = DisabledApplicationReason.NOT_ENOUGH_SHARDS))
        Assert.AreEqual<int>(1, newApp.DisabledReasons |> List.length)

    // TODO: Write tests for `addIntent`, `removeIntent`, and `setIntents` once properly implemented (probably don't just want bitfield stored)

    [<TestMethod>]
    member _.``setShardCount - Sets shard count`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
        let newShardCount = 5

        // Act
        let newApp = Application.setShardCount newShardCount app

        // Assert
        Assert.AreEqual<int>(newShardCount, newApp.ShardCount)

    [<TestMethod>]
    member _.``setHandler - Adds handler to application`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
        let handler = WebhookHandler.create "https://example.com" "ed25519PublicKey" "ed25519PrivateKey" |> Handler.WEBHOOK

        // Act
        let newApp = Application.setHandler handler app

        // Assert
        Assert.AreEqual<Handler option>(Some handler, newApp.Handler)
        
    [<TestMethod>]
    member _.``setHandler - Replaces previous handler on application`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.setHandler (WebhookHandler.create "https://example.com" "ed25519PublicKey" "ed25519PrivateKey" |> Handler.WEBHOOK)
        
        let newHandler = ServiceBusHandler.create "connectionString" "queueName" |> Handler.SERVICE_BUS

        // Act
        let newApp = Application.setHandler newHandler app

        // Assert
        Assert.AreEqual<Handler option>(Some newHandler, newApp.Handler)
        
    [<TestMethod>]
    member _.``removeHandler - Removes existing handler from application`` () =
        // Arrange
        let app =
            Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents
            |> Application.setHandler (ServiceBusHandler.create "connectionString" "queueName" |> Handler.SERVICE_BUS)

        // Act
        let newApp = Application.removeHandler app

        // Assert
        Assert.AreEqual<Handler option>(None, newApp.Handler)

    [<TestMethod>]
    member _.``removeHandler - Changes nothing if application already has no handler`` () =
        // Arrange
        let app = Application.create "id" "encryptedBotToken" _defaultPrivilegedIntents

        // Act
        let newApp = Application.removeHandler app

        // Assert
        Assert.AreEqual<Handler option>(None, newApp.Handler)
