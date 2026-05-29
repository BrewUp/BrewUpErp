namespace BrewUp.Chat.Facade;

public sealed class AzureOpenAiOptions
{
    public const string SectionName = "AzureOpenAI";

    public required string Endpoint { get; init; }

    /// <summary>
    /// API key used when <see cref="UseManagedIdentity"/> is false.
    /// Leave empty in production and rely on Managed Identity.
    /// </summary>
    public string? ApiKey { get; init; }

    public required string DeploymentName { get; init; }

    /// <summary>
    /// When true, authenticates via <c>DefaultAzureCredential</c> (Managed Identity,
    /// Azure CLI, Visual Studio, etc.) instead of <see cref="ApiKey"/>.
    /// Recommended for Azure AI Foundry deployments.
    /// </summary>
    public bool UseManagedIdentity { get; init; }

    /// <summary>
    /// TenantId.
    /// </summary>

    public required string TenantId { get; init; }
}
