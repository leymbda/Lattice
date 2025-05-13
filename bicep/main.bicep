param location string
param clientId string
@secure()
param clientSecret string
@secure()
param botTokenEncryptionKey string

var name = 'lattice'
var uniqueId = substring(uniqueString(name, location), 0, 8)

// Cosmos DB
resource azCosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: '${uniqueId}-${name}-db'
  kind: 'GlobalDocumentDB'
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource azCosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  name: 'lattice-db'
  parent: azCosmosDb
  location: location
  properties: {
    resource: {
      id: 'lattice-db'
    }
  }
}

resource azCosmosDbContainerUsers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  name: 'users'
  parent: azCosmosDbDatabase
  location: location
  properties: {
    resource: {
      id: 'users'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
    }
  }
}

resource azCosmosDbContainerApps 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  name: 'apps'
  parent: azCosmosDbDatabase
  location: location
  properties: {
    resource: {
      id: 'apps'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
    }
  }
}

resource azCosmosDbContainerTeamCache 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  name: 'team-cache'
  parent: azCosmosDbDatabase
  location: location
  properties: {
    resource: {
      id: 'team-cache'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
      defaultTtl: 600 // 10 minutes
    }
  }
}

// Orchestrator API
resource azStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${uniqueId}${name}asa'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
  }
}

resource azStorageBlobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  name: 'default'
  parent: azStorageAccount
}

resource azStorageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  name: name
  parent: azStorageBlobServices
  properties: {
    publicAccess: 'None'
  }
}

resource azServerFarm 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${uniqueId}-${name}-sf'
  location: location
  kind: 'functionapp,linux'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

resource azFunctionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${name}-fa'
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: azServerFarm.id
    httpsOnly: true
    siteConfig: {
      cors: {
        allowedOrigins: ['*']
      }
      use32BitWorkerProcess: false
    }
    functionAppConfig: {
      scaleAndConcurrency: {
        maximumInstanceCount: 100
        instanceMemoryMB: 2048
      }
      runtime: { 
        name: 'dotnet-isolated'
        version: '8.0'
      }
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${azStorageAccount.properties.primaryEndpoints.blob}${azStorageAccount.name}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
    }
  }
}

resource azFunctionAppSystemKey 'Microsoft.Web/sites/host/systemkeys@2021-03-01' = {
  name: '${azFunctionApp.name}/default/webpubsub_extension'
  properties: {
    name: 'webpubsub_extension'
  }
}

var storageRoleDefinitionId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b' // Built-in blob storage role data owner role

resource azRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(azStorageAccount.id, storageRoleDefinitionId)
  scope: azStorageAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', storageRoleDefinitionId)
    principalId: azFunctionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Web PubSub
resource azWebPubSub 'Microsoft.SignalRService/webPubSub@2023-02-01' = {
  name: '${uniqueId}-${name}-wps'
  location: location
  sku: {
    capacity: 1
    name: 'Free_F1'
    tier: 'Free'
  }
  identity: {
    type: 'None'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

var eventHandlerAddress = azFunctionApp.properties.defaultHostName
var extensionKey = listKeys(resourceId('Microsoft.Web/sites/host', azFunctionApp.name, 'default'), '2022-03-01').systemkeys.webpubsub_extension

resource azWebPubSubHub 'Microsoft.SignalRService/webPubSub/hubs@2023-02-01' = {
  name: 'latticehub'
  parent: azWebPubSub
  properties: {
    anonymousConnectPolicy: 'allow'
    eventHandlers: [
      {
        systemEvents: [
          'connected'
          'disconnected'
        ]
        urlTemplate: 'https://${eventHandlerAddress}/runtime/webhooks/webpubsub?code=${extensionKey}'
        userEventPattern: '*'
      }
    ]
  }
}

// Function App Config
resource azFunctionAppConfig 'Microsoft.Web/sites/config@2023-12-01' = {
  name: 'appsettings'
  parent: azFunctionApp
  properties: {
    AzureWebJobsStorage__accountName: azStorageAccount.name
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    
    WebPubSubConnectionString: azWebPubSub.listKeys().primaryConnectionString
    CosmosDb: azCosmosDb.listConnectionStrings().connectionStrings[0].connectionString
    CLIENT_ID: clientId
    CLIENT_SECRET: clientSecret
    BOT_TOKEN_ENCRYPTION_KEY: botTokenEncryptionKey
  }
}

output functionAppName string = azFunctionApp.name
