using System.Text.Json;
using BrewUp.Mother.McpClients;
using BrewUp.Shared.ExternalContracts.Warehouse;

namespace BrewUp.Mother.Facade.Agents;

public sealed class WarehouseAgent(IMcpToolClient mcpToolClient) : IAgent
{
    private const string EvaluateStockImpact = "evaluate-stock-impact";

    public string Name => nameof(WarehouseAgent);
    public string SystemPrompt => "Evaluate stock availability, reorder risk, and fulfillment impact using only Warehouse MCP tools.";

    public IReadOnlyCollection<AgentCapability> Capabilities =>
    [
        new(EvaluateStockImpact, "Evaluates stock impact and reorder risk for a demand signal.")
    ];

    public bool CanHandle(string capabilityName)
        => string.Equals(capabilityName, EvaluateStockImpact, StringComparison.OrdinalIgnoreCase);

    public async Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        var signal = request.Inputs.TryGetValue("salesDemandSignal", out var value)
                     && value is SalesDemandSignal typed
            ? typed
            : new SalesDemandSignal([], 0, 0);

        var impactLines = new List<WarehouseImpactLine>();
        foreach (var demandLine in signal.Lines)
        {
            var availability = await mcpToolClient.CallToolAsync<AvailabilityWithThresholdJson>(
                serverName: "warehouse",
                toolName: "get_beer_availability",
                arguments: new { beerId = demandLine.BeerId },
                cancellationToken);

            var threshold = await mcpToolClient.CallToolAsync<JsonElement>(
                serverName: "warehouse",
                toolName: "get_reorder_thresholds",
                arguments: new { beerId = demandLine.BeerId },
                cancellationToken);

            var availableQuantity = availability?.Quantity ?? 0;
            var reorderThreshold = ExtractThreshold(threshold, availability?.ReorderThreshold ?? 0);
            var remainingQuantity = availableQuantity - demandLine.Quantity;

            impactLines.Add(new WarehouseImpactLine(
                demandLine.BeerId,
                demandLine.BeerName,
                demandLine.Quantity,
                availableQuantity,
                remainingQuantity,
                reorderThreshold,
                StockRisk: remainingQuantity < 0,
                ReorderRisk: remainingQuantity <= reorderThreshold,
                availability?.UnitOfMeasure ?? demandLine.UnitOfMeasure));
        }

        var impact = new WarehouseImpact(
            impactLines.AsReadOnly(),
            impactLines.Any(line => line.StockRisk),
            impactLines.Any(line => line.ReorderRisk));

        return new AgentResponse(
            Name,
            impact.HasStockRisk
                ? "Demand exceeds current warehouse availability for at least one beer."
                : "Demand can be covered by current warehouse availability.",
            new Dictionary<string, object?> { ["warehouseImpact"] = impact },
            impactLines.Count == signal.Lines.Count);
    }

    private static decimal ExtractThreshold(JsonElement threshold, decimal fallback)
    {
        if (threshold.ValueKind == JsonValueKind.Object
            && threshold.TryGetProperty("thresholdQuantity", out var thresholdQuantity)
            && thresholdQuantity.TryGetProperty("value", out var value)
            && value.TryGetDecimal(out var parsed))
            return parsed;

        return fallback;
    }
}
