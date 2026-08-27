namespace BrewUp.Knowledge.SharedKernel.Configuration;

public sealed class AzureOpenAiWikiOptions
{
    public const string SectionName = "BrewUp:AzureOpenAI";

    public string Endpoint { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = string.Empty;
    public string? ApiKey { get; init; }
    public bool UseManagedIdentity { get; init; }
    public string? TenantId { get; init; }
}

