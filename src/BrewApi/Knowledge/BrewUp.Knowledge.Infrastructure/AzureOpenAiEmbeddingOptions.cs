namespace BrewUp.Knowledge.Infrastructure;

public sealed class AzureOpenAiEmbeddingOptions
{
    public const string SectionName = "Knowledge:AzureOpenAI";

    public string Endpoint { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = string.Empty;
    public string? ApiKey { get; init; }
    public bool UseManagedIdentity { get; init; } = true;
    public string? TenantId { get; init; }
}
