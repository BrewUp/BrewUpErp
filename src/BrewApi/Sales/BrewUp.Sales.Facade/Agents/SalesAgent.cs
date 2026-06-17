using BrewUp.Shared.Agents;

namespace BrewUp.Sales.Facade.Agents;

internal sealed class SalesAgent(IMcpToolClient mcpToolClient) : IAgent
{
    private const string InterpretDemand = "interpret-demand-signal";

    public string Name => nameof(SalesAgent);
    public string SystemPrompt => "Interpret customer demand and sales order impact using only Sales MCP tools.";

    public IReadOnlyCollection<AgentCapability> Capabilities =>
    [
        new(InterpretDemand, "Interprets a what-if request as a sales demand signal.")
    ];

    public bool CanHandle(string capabilityName)
        => string.Equals(capabilityName, InterpretDemand, StringComparison.OrdinalIgnoreCase);

    public async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resolved = request.Inputs.TryGetValue("resolvedBeerDemand", out var value)
                       && value is IReadOnlyCollection<ResolvedBeerDemand> typed
            ? typed
            : Array.Empty<ResolvedBeerDemand>();

        var lines = new List<SalesDemandLine>();
        foreach (var item in resolved)
        {
            await mcpToolClient.CallToolAsync<object>(
                serverName: "sales",
                toolName: "get_orders_by_beer",
                arguments: new { beerName = item.Beer.BeerName },
                cancellationToken);

            var unitPrice = item.Beer.Price.Value;
            var lineAmount = item.Demand.Quantity * unitPrice;

            lines.Add(new SalesDemandLine(
                item.Beer.BeerId,
                item.Beer.BeerName,
                item.Demand.Quantity,
                item.Demand.UnitOfMeasure,
                unitPrice,
                lineAmount));
        }

        var signal = new SalesDemandSignal(
            lines.AsReadOnly(),
            lines.Sum(line => line.Quantity),
            lines.Sum(line => line.LineAmount));

        return new AgentResponse(
            Name,
            $"Interpreted demand for {signal.TotalQuantity} bottle(s).",
            new Dictionary<string, object?> { ["salesDemandSignal"] = signal },
            lines.Count > 0);
    }
}
