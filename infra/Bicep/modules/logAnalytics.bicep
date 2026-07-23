@description('Azure region for the Log Analytics workspace.')
param location string

@description('Name of the Log Analytics workspace.')
param name string

@description('Retention in days for workspace logs.')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

@description('Tags applied to the Log Analytics workspace.')
param tags object = {}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: retentionInDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

output id string = workspace.id
output customerId string = workspace.properties.customerId

@secure()
output sharedKey string = workspace.listKeys().primarySharedKey
