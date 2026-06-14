namespace BrewUp.Knowledge.SharedKernel.Configuration;

public sealed class AzureAiSearchOptions
{
    public const string SectionName = "Knowledge:AzureAiSearch";

    public string Endpoint { get; init; } = string.Empty;
    public string IndexName { get; init; } = "brewup-knowledge-chunks";
    public string? ApiKey { get; init; }
    public bool UseManagedIdentity { get; init; } = true;
    public int Dimensions { get; init; } = 1536;
}
