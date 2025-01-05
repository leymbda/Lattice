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
      extensionProfile: {
        extensions: [
          // TODO: Setup docker and run container (here?)
        ]
      }
    }
  }
}

resource autoscalehost 'Microsoft.Insights/autoscalesettings@2022-10-01' = {
  name: 'autoscalehost'
  location: location
  properties: {
    name: 'autoscalehost'
    targetResourceLocation: vmss.id
    enabled: true
    profiles: [{
      name: 'Profile1'
      capacity: {
        minimum: '1'
        maximum: '10'
        default: '1'
      }
      rules: [
        // TODO: Scale in/out rules here
      ]
    }]
  }
}
