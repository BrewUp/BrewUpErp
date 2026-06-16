using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BrewUp.Mother.SharedKernel.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ChatResponse = BrewUp.Mother.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Mother.Facade.Agents;

public sealed class MotherCoordinator(
    IEnumerable<IAgent> agents,
    IEnumerable<IAgentCardProvider>? agentCardProviders = null,
    ILogger<MotherCoordinator>? logger = null)
{
    private static readonly Regex DemandPattern = new(
        @"(?<quantity>\d+(?:[.,]\d+)?)\s*(?:bottles?|bottle|pz|pieces?)\s+(?:of\s+)?(?<beer>[a-zA-Z][a-zA-Z0-9\s'-]*?)(?=\s+(?:and|,)|\?|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IReadOnlyDictionary<string, IAgent> _agents = agents.ToDictionary(a => a.Name);
    private readonly IReadOnlyCollection<IAgentCardProvider> _agentCardProviders =
        agentCardProviders?.ToArray() ?? [];
    private readonly ILogger<MotherCoordinator> _logger = logger ?? NullLogger<MotherCoordinator>.Instance;

    public IReadOnlyCollection<AgentCard> InspectAgentCards()
        => _agentCardProviders
            .Select(provider => provider.GetAgentCard())
            .ToArray();

    public bool CanCoordinate(ChatRequest request)
        => IsWhatIf(request.Message) && ParseDemand(request.Message).Count > 0;

    public async Task<ChatResponse> CoordinateAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var demandItems = ParseDemand(request.Message);
        if (demandItems.Count == 0)
            return new ChatResponse("I could not identify the requested beer quantities in the scenario.", request.ConversationId);

        var correlationId = Guid.CreateVersion7();
        var invokedAgents = new List<string>();
        var context = new AgentContext(
            request.ConversationId ?? string.Empty,
            "Mother",
            invokedAgents.AsReadOnly(),
            new Dictionary<string, object?>());

        var masterDataResponse = await AskAsync(
            nameof(MasterDataAgent),
            "resolve-beer-catalog",
            request.Message,
            new Dictionary<string, object?> { ["demandItems"] = demandItems },
            correlationId,
            context,
            invokedAgents,
            cancellationToken);

        var resolved = masterDataResponse.GetRequired<IReadOnlyCollection<ResolvedBeerDemand>>("resolvedBeerDemand");

        if (resolved.Count == 0)
            return new ChatResponse(
                "BrewUp ERP does not expose the requested product or beer information yet.",
                request.ConversationId);

        var salesResponse = await AskAsync(
            nameof(SalesAgent),
            "interpret-demand-signal",
            request.Message,
            new Dictionary<string, object?> { ["resolvedBeerDemand"] = resolved },
            correlationId,
            context,
            invokedAgents,
            cancellationToken);

        var salesSignal = salesResponse.GetRequired<SalesDemandSignal>("salesDemandSignal");

        var warehouseResponse = await AskAsync(
            nameof(WarehouseAgent),
            "evaluate-stock-impact",
            request.Message,
            new Dictionary<string, object?> { ["salesDemandSignal"] = salesSignal },
            correlationId,
            context,
            invokedAgents,
            cancellationToken);

        var knowledgeResponse = await AskAsync(
            nameof(KnowledgeAgent),
            "retrieve-business-knowledge",
            request.Message,
            new Dictionary<string, object?> { ["resolvedBeerDemand"] = resolved },
            correlationId,
            context,
            invokedAgents,
            cancellationToken);

        var warehouseImpact = warehouseResponse.GetRequired<WarehouseImpact>("warehouseImpact");
        var knowledgeResult = knowledgeResponse.GetRequired<KnowledgeResult>("knowledgeResult");

        return new ChatResponse(
            BuildBusinessAnswer(resolved, salesSignal, warehouseImpact, knowledgeResult),
            request.ConversationId);
    }

    private async Task<AgentResponse> AskAsync(
        string agentName,
        string capability,
        string originalQuestion,
        IReadOnlyDictionary<string, object?> inputs,
        Guid correlationId,
        AgentContext context,
        ICollection<string> invokedAgents,
        CancellationToken cancellationToken)
    {
        if (!_agents.TryGetValue(agentName, out var agent))
            throw new InvalidOperationException($"{agentName} is not registered.");

        if (!agent.CanHandle(capability))
            throw new InvalidOperationException($"{agentName} cannot handle capability '{capability}'.");

        _logger.LogInformation(
            "Mother delegating to {AgentName} for {Capability} with correlation {CorrelationId}",
            agentName,
            capability,
            correlationId);

        invokedAgents.Add(agentName);

        var response = await agent.HandleAsync(
            new AgentRequest(capability, originalQuestion, inputs, correlationId, context),
            cancellationToken);

        _logger.LogInformation(
            "Mother received {AgentName} result for {Capability}: {Summary}",
            response.AgentName,
            capability,
            response.Summary);

        return response;
    }

    private static bool IsWhatIf(string message)
        => message.Contains("what if", StringComparison.OrdinalIgnoreCase)
           || message.Contains("what happens if", StringComparison.OrdinalIgnoreCase)
           || message.Contains("impact", StringComparison.OrdinalIgnoreCase)
           || message.Contains("simulation", StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyCollection<DemandItem> ParseDemand(string message)
        => DemandPattern
            .Matches(message)
            .Select(match => new DemandItem(
                match.Groups["beer"].Value.Trim(),
                decimal.Parse(match.Groups["quantity"].Value.Replace(',', '.'), CultureInfo.InvariantCulture),
                "Bottle"))
            .ToArray();

    private static string BuildBusinessAnswer(
        IReadOnlyCollection<ResolvedBeerDemand> resolved,
        SalesDemandSignal salesSignal,
        WarehouseImpact warehouseImpact,
        KnowledgeResult knowledgeResult)
    {
        var answer = new StringBuilder();
        answer.AppendLine($"Scenario interpreted as demand for {salesSignal.TotalQuantity} bottle(s), estimated value {salesSignal.EstimatedAmount:0.00}.");

        foreach (var line in warehouseImpact.Lines)
        {
            answer.Append("- ");
            answer.Append(line.BeerName);
            answer.Append($": requested {line.RequestedQuantity:0.##}, available {line.AvailableQuantity:0.##}, remaining {line.RemainingQuantity:0.##}");

            if (line.StockRisk)
                answer.Append(". Stock risk: demand exceeds availability");
            else if (line.ReorderRisk)
                answer.Append($". Reorder risk: remaining stock is at or below threshold {line.ReorderThreshold:0.##}");
            else
                answer.Append(". No immediate stock or reorder risk");

            answer.AppendLine(".");
        }

        if (knowledgeResult.Findings.Count > 0)
        {
            var policy = knowledgeResult.Findings.First();
            answer.AppendLine($"Documented rule: {policy.Title}: {policy.Content}");
        }
        else
        {
            answer.AppendLine("Documented rule: BrewUp ERP does not expose that information yet.");
        }

        var unresolvedCount = salesSignal.Lines.Count == 0
            ? 0
            : resolved.Count(item => string.IsNullOrWhiteSpace(item.Beer.BeerId));

        if (warehouseImpact.HasStockRisk)
            answer.Append("Recommendation: do not confirm the full scenario before checking replenishment or partial fulfillment options.");
        else if (warehouseImpact.HasReorderRisk)
            answer.Append("Recommendation: the order can be considered, but purchasing/production should review replenishment.");
        else
            answer.Append("Recommendation: the warehouse can absorb this scenario with current stock.");

        if (unresolvedCount > 0)
            answer.Append($" {unresolvedCount} requested beer(s) could not be resolved in MasterData.");

        return answer.ToString();
    }
}
