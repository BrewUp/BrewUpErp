@description('Azure region for the container registry.')
param location string

@description('Name of the Azure Container Registry.')
param name string

@description('Tags applied to the container registry.')
param tags object = {}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
  }
}

var credentials = registry.listCredentials()

output name string = registry.name
output loginServer string = registry.properties.loginServer
output adminUsername string = credentials.username

@secure()
output adminPassword string = credentials.passwords[0].value
