using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Mother.McpClients;

namespace BrewUp.Mother.Facade.Agents;

public sealed class KnowledgeAgent(IMcpToolClient mcpToolClient) : IAgent
{
    private const string RetrieveBusinessKnowledge = "retrieve-business-knowledge";

    public string Name => nameof(KnowledgeAgent);
    public string SystemPrompt => "Retrieve documented business knowledge, policies, procedures, and operational rules using only Knowledge MCP tools.";

    public IReadOnlyCollection<AgentCapability> Capabilities =>
    [
        new(RetrieveBusinessKnowledge, "Retrieves documented policies and operational rules relevant to a scenario.")
    ];

    public bool CanHandle(string capabilityName)
        => string.Equals(capabilityName, RetrieveBusinessKnowledge, StringComparison.OrdinalIgnoreCase);

    public async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        var resolved = request.Inputs.TryGetValue("resolvedBeerDemand", out var value)
                       && value is IReadOnlyCollection<ResolvedBeerDemand> typed
            ? typed
            : Array.Empty<ResolvedBeerDemand>();

        var beerNames = resolved.Count > 0
            ? string.Join(", ", resolved.Select(item => item.Beer.BeerName))
            : "requested beers";

        var searchResult = await mcpToolClient.CallToolAsync<SearchKnowledgeResult>(
            serverName: "knowledge",
            toolName: "search_knowledge_base",
            arguments: new
            {
                query = $"{beerNames} reorder policy stock threshold fulfillment rules",
                scope = "Warehouse",
                topK = 3
            },
            cancellationToken);

        var findings = searchResult?.Items
            .Select(item => new KnowledgeFinding(
                item.DocumentTitle,
                item.Scope,
                item.Content,
                item.Score))
            .ToArray()
            ?? [];

        var result = new KnowledgeResult(findings);

        return new AgentResponse(
            Name,
            findings.Length > 0
                ? $"Retrieved {findings.Length} documented business knowledge item(s)."
                : "BrewUp ERP does not expose documented business knowledge for this scenario yet.",
            new Dictionary<string, object?> { ["knowledgeResult"] = result },
            findings.Length > 0);
    }
}
