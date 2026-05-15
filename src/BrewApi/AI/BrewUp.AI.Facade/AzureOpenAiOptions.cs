namespace BrewUp.AI.Facade;

public sealed class AzureOpenAiOptions
{
    public const string SectionName = "AzureOpenAI";

    public required string Endpoint { get; init; }
    public required string ApiKey { get; init; }
    public required string DeploymentName { get; init; }
}
