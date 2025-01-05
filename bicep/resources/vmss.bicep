param name string
param location string
param vmSku string
param instanceCount int

param adminUsername string
@secure()
param adminPassword string

var vmssName = 'vmss-${name}'
var cloudInit = base64(loadTextContent('../../bin/cloudinit.yml'))

resource vnet 'Microsoft.Network/virtualNetworks@2020-11-01' = {
  name: 'vnet-${name}'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: ['10.0.0.0/16']
    }
    subnets: [
      {
        name: 'sn-${name}'
        properties: {
          addressPrefix: '10.0.0.0/24'
        }
      }
    ]
  }
}

resource vmss 'Microsoft.Compute/virtualMachineScaleSets@2024-07-01' = {
  name: vmssName
  location: location
  sku: {
    name: vmSku
    tier: 'Standard'
    capacity: instanceCount
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    overprovision: true
    upgradePolicy: {
      mode: 'Manual'
    }
    virtualMachineProfile: {
      storageProfile: {
        osDisk: {
          caching: 'ReadWrite'
          createOption: 'FromImage'
          diskSizeGB: 10
          managedDisk: {
            storageAccountType: 'Standard_LRS'
          }
        }
        imageReference: {
          publisher: 'Canonical'
          offer: '0001-com-ubuntu-server-jammy'
          sku: '22_04-lts-gen2'
          version: 'latest'
        }
      }
      osProfile: {
        computerNamePrefix: vmssName
        adminUsername: adminUsername
        adminPassword: adminPassword
        customData: cloudInit
      }
      networkProfile: {
        networkInterfaceConfigurations: [
          {
            name: 'nic-${vmssName}'
            properties: {
              primary: true
              ipConfigurations: [
                {
                  name: 'ipconfig-${vmssName}'
                  properties: {
                    subnet: {
                      id: vnet.properties.subnets[0].id
                    }
                    primary: true
                  }
                }
              ]
            }
          }
        ]
      }
    }
  }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: 'ra-${name}'
  scope: vmss
  properties: {
    principalId: vmss.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
  }
}

// resource autoscalehost 'Microsoft.Insights/autoscalesettings@2022-10-01' = {
//   name: 'autoscalehost'
//   location: location
//   properties: {
//     name: 'autoscalehost'
//     targetResourceLocation: vmss.id
//     enabled: true
//     profiles: [{
//       name: 'Profile1'
//       capacity: {
//         minimum: '1'
//         maximum: '10'
//         default: '1'
//       }
//       rules: [
//         // TODO: Scale in/out rules here
//       ]
//     }]
//   }
// }
