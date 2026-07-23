# BrewUp Knowledge Azure Deployment

This folder contains the first Bicep deployment for BrewUp Knowledge.

## Resources

- Azure Container Registry
- Log Analytics Workspace
- Azure Container Apps Environment
- Public Knowledge Agent Container App
- Public Knowledge MCP Container App
- Public Sales MCP Container App
- Public Warehouse MCP Container App
- Public MasterData MCP Container App

The deployment intentionally does not create SQL Server, Azure OpenAI, Azure AI Search, Key Vault, Mother, or other BrewUp services.

## Build and Deploy

Use the existing resource group:

```powershell
$resourceGroupName = "BrewUpResourceGroup"
```

The deployment accepts the same important configuration keys you use in `src/BrewApi/.env`, but the values should be passed as deployment parameters instead of committed into Bicep:

| `.env` key | Bicep parameter | Notes |
| --- | --- | --- |
| `BrewUp__MongoDbSettings__ConnectionString` | `mongoDbConnectionString` | Stored as a Container App secret for Sales, Warehouse, and MasterData MCP apps. |
| `BrewUp__MongoDbSettings__DatabaseName` | `mongoDbDatabaseName` | Normal env var for Sales, Warehouse, and MasterData MCP apps. |
| `BrewUp__SqlServer__ConnectionString` | `sqlServerConnectionString` | Stored as a Container App secret for Knowledge MCP. |
| `BrewUp__SqlServer__Dimensions` | `sqlServerDimensions` | Normal env var for Knowledge MCP. |
| `Knowledge__VectorStore` | `knowledgeVectorStore` | Defaults to `SqlServer`. |
| `KnowledgeAgent__Mcp__ServerName` | `knowledgeAgentMcpServerName` | Defaults to `knowledge`. |
| `KnowledgeAgent__Mcp__DefaultTopK` | `knowledgeAgentDefaultTopK` | Defaults to `5`. |

For a brand-new registry, bootstrap the shared resources first:

```powershell
az deployment group create `
  --resource-group $resourceGroupName `
  --template-file infra/main.bicep `
  --parameters infra/main.parameters.example.json `
  --parameters deployContainerApps=false
```

Push the existing images to the deployed registry:

```powershell
az acr login --name brewupregistry

docker tag brewup.knowledge.agent:latest brewupregistry.azurecr.io/brewup.knowledge.agent:latest
docker tag brewup.knowledge.mcpserver:latest brewupregistry.azurecr.io/brewup.knowledge.mcpserver:latest
docker tag brewup.sales.mcpserver:latest brewupregistry.azurecr.io/brewup.sales.mcpserver:latest
docker tag brewup.warehouse.mcpserver:latest brewupregistry.azurecr.io/brewup.warehouse.mcpserver:latest
docker tag brewup.masterdata.mcpserver:latest brewupregistry.azurecr.io/brewup.masterdata.mcpserver:latest

docker push brewupregistry.azurecr.io/brewup.knowledge.agent:latest
docker push brewupregistry.azurecr.io/brewup.knowledge.mcpserver:latest
docker push brewupregistry.azurecr.io/brewup.sales.mcpserver:latest
docker push brewupregistry.azurecr.io/brewup.warehouse.mcpserver:latest
docker push brewupregistry.azurecr.io/brewup.masterdata.mcpserver:latest
```

Deploy the Container Apps after the images are available:

```powershell
$mongoConnectionString = "<value from BrewUp__MongoDbSettings__ConnectionString>"
$sqlConnectionString = "<value from BrewUp__SqlServer__ConnectionString>"

az deployment group create `
  --resource-group $resourceGroupName `
  --template-file infra/main.bicep `
  --parameters infra/main.parameters.example.json `
  --parameters mongoDbConnectionString="$mongoConnectionString" `
  --parameters sqlServerConnectionString="$sqlConnectionString"
```

The app URLs and MCP endpoints are emitted as deployment outputs.
