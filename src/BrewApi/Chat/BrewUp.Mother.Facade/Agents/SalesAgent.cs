namespace BrewUp.Mother.Facade.Agents;

public sealed class SalesAgent : IAgent
{
    private const string InterpretDemand = "interpret-demand-signal";

    public string Name => nameof(SalesAgent);

    public IReadOnlyCollection<AgentCapability> Capabilities =>
    [
        new(InterpretDemand, "Interprets a what-if request as a sales demand signal.")
    ];

    public bool CanHandle(string capabilityName)
        => string.Equals(capabilityName, InterpretDemand, StringComparison.OrdinalIgnoreCase);

    public Task<AgentResponse> HandleAsync(AgentRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resolved = request.Inputs.TryGetValue("resolvedBeerDemand", out var value)
                       && value is IReadOnlyCollection<ResolvedBeerDemand> typed
            ? typed
            : Array.Empty<ResolvedBeerDemand>();

        var lines = resolved
            .Select(item =>
            {
                var unitPrice = item.Beer.Price.Value;
                var lineAmount = item.Demand.Quantity * unitPrice;

                return new SalesDemandLine(
                    item.Beer.BeerId,
                    item.Beer.BeerName,
                    item.Demand.Quantity,
                    item.Demand.UnitOfMeasure,
                    unitPrice,
                    lineAmount);
            })
            .ToArray();

        var signal = new SalesDemandSignal(
            lines,
            lines.Sum(line => line.Quantity),
            lines.Sum(line => line.LineAmount));

        return Task.FromResult(new AgentResponse(
            Name,
            $"Interpreted demand for {signal.TotalQuantity} bottle(s).",
            new Dictionary<string, object?> { ["salesDemandSignal"] = signal },
            lines.Length > 0));
    }
}
