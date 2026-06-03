<#
.SYNOPSIS
    Provisions an Azure AI Foundry resource and a gpt-4o-mini deployment for BrewUp Chat.

.DESCRIPTION
    Creates (or reuses):
      - Resource group
      - Azure AI Foundry account (kind = AIServices, SKU = S0)
      - Foundry project
      - gpt-4o-mini deployment (GlobalStandard SKU)
      - RBAC: assigns "Cognitive Services OpenAI User" to the caller and (optionally)
        to a managed identity / service principal you pass in.

    Idempotent: re-running the script reconciles state instead of failing.

.EXAMPLE
    ./provision-foundry.ps1 -ResourceGroup brewup-ai-rg -Location swedencentral -ResourceName brewup-foundry

.NOTES
    Requires: az CLI >= 2.61, logged in via `az login`, contributor on the subscription.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $ResourceGroup,
    [Parameter(Mandatory = $true)] [string] $ResourceName,
    [string] $Location        = "swedencentral",
    [string] $ProjectName     = "brewup-chat",
    [string] $ModelName       = "gpt-4o-mini",
    [string] $ModelVersion    = "2024-07-18",
    [string] $DeploymentName  = "brewup-chat",
    [string] $DeploymentSku   = "GlobalStandard",
    [int]    $DeploymentCapacity = 50,
    [string] $PrincipalIdToGrant
)

$ErrorActionPreference = "Stop"

function Invoke-Az([string[]] $Args) {
    Write-Host ">> az $($Args -join ' ')" -ForegroundColor DarkGray
    & az @Args
    if ($LASTEXITCODE -ne 0) { throw "az command failed: $($Args -join ' ')" }
}

Write-Host "== Resource Group ==" -ForegroundColor Cyan
Invoke-Az @("group", "create", "--name", $ResourceGroup, "--location", $Location, "--output", "none")

Write-Host "== Foundry account ($ResourceName) ==" -ForegroundColor Cyan
Invoke-Az @(
    "cognitiveservices", "account", "create",
    "--name", $ResourceName,
    "--resource-group", $ResourceGroup,
    "--location", $Location,
    "--kind", "AIServices",
    "--sku", "S0",
    "--custom-domain", $ResourceName,
    "--assign-identity",
    "--output", "none"
)

Write-Host "== Foundry project ($ProjectName) ==" -ForegroundColor Cyan
# Foundry projects use the new 'cognitiveservices account project' surface (preview commands kept stable as of 2026).
$existingProject = az cognitiveservices account project show `
    --name $ProjectName `
    --account-name $ResourceName `
    --resource-group $ResourceGroup `
    --output tsv 2>$null
if (-not $existingProject) {
    Invoke-Az @(
        "cognitiveservices", "account", "project", "create",
        "--name", $ProjectName,
        "--account-name", $ResourceName,
        "--resource-group", $ResourceGroup,
        "--output", "none"
    )
} else {
    Write-Host "Project already exists, skipping." -ForegroundColor Yellow
}

Write-Host "== Model deployment ($DeploymentName -> $ModelName:$ModelVersion) ==" -ForegroundColor Cyan
Invoke-Az @(
    "cognitiveservices", "account", "deployment", "create",
    "--name", $ResourceName,
    "--resource-group", $ResourceGroup,
    "--deployment-name", $DeploymentName,
    "--model-name", $ModelName,
    "--model-version", $ModelVersion,
    "--model-format", "OpenAI",
    "--sku-name", $DeploymentSku,
    "--sku-capacity", "$DeploymentCapacity",
    "--output", "none"
)

# Endpoint that the OpenAI SDK / Microsoft.Extensions.AI uses:
$endpoint = "https://$ResourceName.openai.azure.com/"

# RBAC: 'Cognitive Services OpenAI User' lets you call inference without API keys.
$openAiUserRoleId = "5e0bd9bd-7b93-4f28-af87-19fc36ad61bd"
$scope = az cognitiveservices account show --name $ResourceName --resource-group $ResourceGroup --query id -o tsv
$callerObjectId = az ad signed-in-user show --query id -o tsv 2>$null

Write-Host "== RBAC assignments ==" -ForegroundColor Cyan
if ($callerObjectId) {
    Invoke-Az @(
        "role", "assignment", "create",
        "--assignee-object-id", $callerObjectId,
        "--assignee-principal-type", "User",
        "--role", $openAiUserRoleId,
        "--scope", $scope,
        "--output", "none"
    )
}
if ($PrincipalIdToGrant) {
    Invoke-Az @(
        "role", "assignment", "create",
        "--assignee-object-id", $PrincipalIdToGrant,
        "--assignee-principal-type", "ServicePrincipal",
        "--role", $openAiUserRoleId,
        "--scope", $scope,
        "--output", "none"
    )
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Green
Write-Host "Foundry ready." -ForegroundColor Green
Write-Host "Endpoint:        $endpoint"
Write-Host "DeploymentName:  $DeploymentName"
Write-Host "Use Managed Identity (no API key) in appsettings:"
Write-Host @"

  "AzureOpenAI": {
    "Endpoint": "$endpoint",
    "DeploymentName": "$DeploymentName",
    "UseManagedIdentity": true
  }

"@
Write-Host "==================================================" -ForegroundColor Green

