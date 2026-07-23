targetScope = 'resourceGroup'

@description('Azure region for all BrewUp Knowledge resources.')
param location string = resourceGroup().location

@description('Short environment name used in resource names.')
@allowed([
  'dev'
  'test'
  'prod'
])
param environmentName string = 'dev'

@description('Application/workload prefix used in resource names.')
param workloadName string = 'brewup-knowledge'

@description('Globally unique Azure Container Registry name. Use only lowercase letters and numbers.')
@minLength(5)
@maxLength(50)
param containerRegistryName string = toLower(replace('${workloadName}${environmentName}${uniqueString(resourceGroup().id)}', '-', ''))

@description('Container image tag to deploy for all BrewUp images.')
param imageTag string = 'latest'

@description('Deploy Container Apps. Set false for the first bootstrap pass when the ACR must be created before images can be pushed.')
param deployContainerApps bool = true

@secure()
@description('Existing MongoDB connection string used by Sales, Warehouse, and MasterData MCP servers.')
param mongoDbConnectionString string = ''

@description('Existing MongoDB database name used by Sales, Warehouse, and MasterData MCP servers.')
param mongoDbDatabaseName string = 'BrewUp'

@secure()
@description('Existing SQL Server connection string used by the Knowledge MCP vector store.')
param sqlServerConnectionString string = ''

@description('Vector dimensions used by the Knowledge SQL vector store.')
param sqlServerDimensions int = 1536

@description('Knowledge MCP vector store implementation.')
@allowed([
  'SqlServer'
  'AzureAiSearch'
])
param knowledgeVectorStore string = 'SqlServer'

@description('Knowledge Agent MCP server name.')
param knowledgeAgentMcpServerName string = 'knowledge'

@description('Knowledge Agent default top-K retrieval setting.')
param knowledgeAgentDefaultTopK int = 5

@description('Minimum replicas for each Container App.')
@minValue(0)
param minReplicas int = 1

@description('Maximum replicas for each Container App.')
@minValue(1)
param maxReplicas int = 3

@description('Tags applied to all resources.')
param tags object = {
  workload: 'BrewUp Knowledge'
  environment: environmentName
}

var normalizedWorkloadName = toLower(workloadName)
var logAnalyticsName = take('log-${normalizedWorkloadName}-${environmentName}', 63)
var containerAppsEnvironmentName = take('cae-${normalizedWorkloadName}-${environmentName}', 32)

var knowledgeMcpName = take('ca-${normalizedWorkloadName}-mcp-${environmentName}', 32)
var knowledgeAgentName = take('ca-${normalizedWorkloadName}-agent-${environmentName}', 32)
var salesMcpName = take('ca-brewup-sales-mcp-${environmentName}', 32)
var warehouseMcpName = take('ca-brewup-warehouse-mcp-${environmentName}', 32)
var masterDataMcpName = take('ca-brewup-masterdata-mcp-${environmentName}', 32)

module registry 'modules/containerRegistry.bicep' = {
  name: 'containerRegistry'
  params: {
    location: location
    name: containerRegistryName
    tags: tags
  }
}

module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics'
  params: {
    location: location
    name: logAnalyticsName
    tags: tags
  }
}

module containerAppsEnvironment 'modules/containerAppsEnvironment.bicep' = {
  name: 'containerAppsEnvironment'
  params: {
    location: location
    name: containerAppsEnvironmentName
    logAnalyticsCustomerId: logAnalytics.outputs.customerId
    logAnalyticsSharedKey: logAnalytics.outputs.sharedKey
    tags: tags
  }
}

var knowledgeMcpEndpoint = 'https://${knowledgeMcpName}.${containerAppsEnvironment.outputs.defaultDomain}/mcp'
var salesMcpEndpoint = 'https://${salesMcpName}.${containerAppsEnvironment.outputs.defaultDomain}/mcp'
var warehouseMcpEndpoint = 'https://${warehouseMcpName}.${containerAppsEnvironment.outputs.defaultDomain}/mcp'
var masterDataMcpEndpoint = 'https://${masterDataMcpName}.${containerAppsEnvironment.outputs.defaultDomain}/mcp'
var mongoDbEnvironmentVariables = [
  {
    name: 'BrewUp__MongoDbSettings__DatabaseName'
    value: mongoDbDatabaseName
  }
]
var mongoDbSecretEnvironmentVariables = [
  {
    name: 'BrewUp__MongoDbSettings__ConnectionString'
    secretRef: 'mongodb-connection-string'
  }
]
var mongoDbSecrets = {
  'mongodb-connection-string': mongoDbConnectionString
}
var knowledgeMcpEnvironmentVariables = [
  {
    name: 'BrewUp__SqlServer__Dimensions'
    value: string(sqlServerDimensions)
  }
  {
    name: 'Knowledge__VectorStore'
    value: knowledgeVectorStore
  }
]
var knowledgeMcpSecretEnvironmentVariables = [
  {
    name: 'BrewUp__SqlServer__ConnectionString'
    secretRef: 'sqlserver-connection-string'
  }
]
var knowledgeMcpSecrets = {
  'sqlserver-connection-string': sqlServerConnectionString
}

module knowledgeMcp 'modules/containerApp.bicep' = if (deployContainerApps) {
  name: 'knowledgeMcpContainerApp'
  params: {
    location: location
    name: knowledgeMcpName
    environmentId: containerAppsEnvironment.outputs.id
    environmentDefaultDomain: containerAppsEnvironment.outputs.defaultDomain
    imageName: 'brewup.knowledge.mcpserver'
    imageTag: imageTag
    targetPort: 8080
    externalIngress: true
    registryServer: registry.outputs.loginServer
    registryUsername: registry.outputs.adminUsername
    registryPassword: registry.outputs.adminPassword
    environmentVariables: knowledgeMcpEnvironmentVariables
    secrets: knowledgeMcpSecrets
    secretEnvironmentVariables: knowledgeMcpSecretEnvironmentVariables
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    tags: tags
  }
}

module salesMcp 'modules/containerApp.bicep' = if (deployContainerApps) {
  name: 'salesMcpContainerApp'
  params: {
    location: location
    name: salesMcpName
    environmentId: containerAppsEnvironment.outputs.id
    environmentDefaultDomain: containerAppsEnvironment.outputs.defaultDomain
    imageName: 'brewup.sales.mcpserver'
    imageTag: imageTag
    targetPort: 8080
    externalIngress: true
    registryServer: registry.outputs.loginServer
    registryUsername: registry.outputs.adminUsername
    registryPassword: registry.outputs.adminPassword
    environmentVariables: concat(mongoDbEnvironmentVariables, [
      {
        name: 'SalesAgent__Mcp__Endpoint'
        value: salesMcpEndpoint
      }
    ])
    secrets: mongoDbSecrets
    secretEnvironmentVariables: mongoDbSecretEnvironmentVariables
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    tags: tags
  }
}

module warehouseMcp 'modules/containerApp.bicep' = if (deployContainerApps) {
  name: 'warehouseMcpContainerApp'
  params: {
    location: location
    name: warehouseMcpName
    environmentId: containerAppsEnvironment.outputs.id
    environmentDefaultDomain: containerAppsEnvironment.outputs.defaultDomain
    imageName: 'brewup.warehouse.mcpserver'
    imageTag: imageTag
    targetPort: 8080
    externalIngress: true
    registryServer: registry.outputs.loginServer
    registryUsername: registry.outputs.adminUsername
    registryPassword: registry.outputs.adminPassword
    environmentVariables: concat(mongoDbEnvironmentVariables, [
      {
        name: 'WarehouseAgent__Mcp__Endpoint'
        value: warehouseMcpEndpoint
      }
    ])
    secrets: mongoDbSecrets
    secretEnvironmentVariables: mongoDbSecretEnvironmentVariables
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    tags: tags
  }
}

module masterDataMcp 'modules/containerApp.bicep' = if (deployContainerApps) {
  name: 'masterDataMcpContainerApp'
  params: {
    location: location
    name: masterDataMcpName
    environmentId: containerAppsEnvironment.outputs.id
    environmentDefaultDomain: containerAppsEnvironment.outputs.defaultDomain
    imageName: 'brewup.masterdata.mcpserver'
    imageTag: imageTag
    targetPort: 8080
    externalIngress: true
    registryServer: registry.outputs.loginServer
    registryUsername: registry.outputs.adminUsername
    registryPassword: registry.outputs.adminPassword
    environmentVariables: concat(mongoDbEnvironmentVariables, [
      {
        name: 'MasterData__Mcp__Endpoint'
        value: masterDataMcpEndpoint
      }
    ])
    secrets: mongoDbSecrets
    secretEnvironmentVariables: mongoDbSecretEnvironmentVariables
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    tags: tags
  }
}

module knowledgeAgent 'modules/containerApp.bicep' = if (deployContainerApps) {
  name: 'knowledgeAgentContainerApp'
  params: {
    location: location
    name: knowledgeAgentName
    environmentId: containerAppsEnvironment.outputs.id
    environmentDefaultDomain: containerAppsEnvironment.outputs.defaultDomain
    imageName: 'brewup.knowledge.agent'
    imageTag: imageTag
    targetPort: 8080
    externalIngress: true
    registryServer: registry.outputs.loginServer
    registryUsername: registry.outputs.adminUsername
    registryPassword: registry.outputs.adminPassword
    environmentVariables: [
      {
        name: 'KnowledgeAgent__Mcp__ServerName'
        value: knowledgeAgentMcpServerName
      }
      {
        name: 'KnowledgeAgent__Mcp__Endpoint'
        value: knowledgeMcpEndpoint
      }
      {
        name: 'KnowledgeAgent__Mcp__DefaultTopK'
        value: string(knowledgeAgentDefaultTopK)
      }
    ]
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    tags: tags
  }
  dependsOn: [
    knowledgeMcp
  ]
}

output containerRegistryLoginServer string = registry.outputs.loginServer
output knowledgeAgentUrl string = deployContainerApps ? knowledgeAgent!.outputs.url : ''
output knowledgeMcpUrl string = deployContainerApps ? knowledgeMcp!.outputs.url : ''
output knowledgeMcpEndpoint string = deployContainerApps ? knowledgeMcpEndpoint : ''
output salesMcpUrl string = deployContainerApps ? salesMcp!.outputs.url : ''
output salesMcpEndpoint string = deployContainerApps ? salesMcpEndpoint : ''
output warehouseMcpUrl string = deployContainerApps ? warehouseMcp!.outputs.url : ''
output warehouseMcpEndpoint string = deployContainerApps ? warehouseMcpEndpoint : ''
output masterDataMcpUrl string = deployContainerApps ? masterDataMcp!.outputs.url : ''
output masterDataMcpEndpoint string = deployContainerApps ? masterDataMcpEndpoint : ''
