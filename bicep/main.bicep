param adminUsername string
@secure()
param adminPassword string

module vmss 'resources/vmss.bicep' = {
  name: 'vmss'
  params: {
    name: 'pool-lattice'
    location: resourceGroup().location
    vmSku: 'Standard_B1ls'
    instanceCount: 1
    adminUsername: adminUsername
    adminPassword: adminPassword
  }
}
