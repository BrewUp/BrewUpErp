namespace BrewUp.Knowledge.Agent.Tools;

public sealed class KnowledgeAgentMcpOptions
{
    public string ServerName { get; set; } = "knowledge";

    public string? Endpoint { get; set; }

    public int DefaultTopK { get; set; } = 5;
}
