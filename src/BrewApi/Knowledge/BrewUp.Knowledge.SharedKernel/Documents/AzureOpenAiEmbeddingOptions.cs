namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed class AzureOpenAiEmbeddingOptions
{
    public const string SectionName = "BrewUp:Embeddings";

    public string Endpoint { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = string.Empty;
    public int Dimensions { get; init; } = 1536;
    public string? ApiKey { get; init; }
    public bool UseManagedIdentity { get; init; } = true;
    public string? TenantId { get; init; }
}
