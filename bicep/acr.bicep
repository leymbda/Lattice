param principalId string

var name = 'acr-lattice'
var roleId = '8311e382-0749-4cb8-b61a-304f252e45ec' // AcrPush role

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: name
  location: resourceGroup().location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

// resource registryRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
//   name: guid(subscription().subscriptionId, resourceGroup().name, name, roleId, principalId)
//   scope: acr
//   properties: {
//     roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleId)
//     principalId: principalId
//   }
// }

output loginServer string = acr.properties.loginServer
output name string = acr.name
