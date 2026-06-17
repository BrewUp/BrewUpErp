using BrewUp.Shared.Agents;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;

namespace BrewUp.MasterData.Facade.Agents;

internal sealed class MasterDataAgent(IMcpToolClient mcpToolClient) : IAgent
{
    private const string ResolveBeerCatalog = "resolve-beer-catalog";

    public string Name => nameof(MasterDataAgent);
    public string SystemPrompt => "Resolve BrewUp products, beers, beer styles, customers, and suppliers using only MasterData MCP tools.";

    public IReadOnlyCollection<AgentCapability> Capabilities =>
    [
        new(ResolveBeerCatalog, "Resolves beer names into MasterData catalog records.")
    ];

    public bool CanHandle(string capabilityName)
        => string.Equals(capabilityName, ResolveBeerCatalog, StringComparison.OrdinalIgnoreCase);

    public async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        var demandItems = request.Inputs.TryGetValue("demandItems", out var value)
                          && value is IReadOnlyCollection<DemandItem> typed
            ? typed
            : Array.Empty<DemandItem>();

        var resolved = new List<ResolvedBeerDemand>();
        foreach (var demandItem in demandItems)
        {
            var beer = await mcpToolClient.CallToolAsync<BeerJson>(
                serverName: "masterData",
                toolName: "resolve-beer-catalog",
                arguments: new { beerName = demandItem.BeerName },
                cancellationToken);

            if (beer is null || string.IsNullOrWhiteSpace(beer.BeerId))
                continue;

            resolved.Add(new ResolvedBeerDemand(demandItem, beer));
        }

        return new AgentResponse(
            Name,
            $"Resolved {resolved.Count} beer catalog item(s).",
            new Dictionary<string, object?> { ["resolvedBeerDemand"] = resolved.AsReadOnly() },
            resolved.Count == demandItems.Count);
    }
}
