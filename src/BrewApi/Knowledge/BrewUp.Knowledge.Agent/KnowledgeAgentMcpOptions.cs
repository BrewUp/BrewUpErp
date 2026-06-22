namespace BrewUp.Knowledge.Agent;

public sealed class KnowledgeAgentMcpOptions
{
    public string ServerName { get; set; } = "knowledge";

    public string? Endpoint { get; set; }

    public int DefaultTopK { get; set; } = 5;
}
