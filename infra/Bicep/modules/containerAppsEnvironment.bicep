@description('Azure region for the Container Apps environment.')
param location string

@description('Name of the Container Apps environment.')
param name string

@description('Log Analytics workspace customer ID.')
param logAnalyticsCustomerId string

@secure()
@description('Log Analytics workspace shared key.')
param logAnalyticsSharedKey string

@description('Tags applied to the Container Apps environment.')
param tags object = {}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsSharedKey
      }
    }
  }
}

output id string = environment.id
output defaultDomain string = environment.properties.defaultDomain
