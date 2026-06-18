namespace BrewUp.Mother.Facade.Agents;

public sealed class MotherA2AOptions
{
    public const string SectionName = "BrewUp:Mother:A2A";

    public bool Enabled { get; init; }
    public string KnowledgeAgentUrl { get; init; } = string.Empty;
}
