using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BrewUp.Mother.Facade.Telemetry;
using BrewUp.Mother.SharedKernel.Chat;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ChatResponse = BrewUp.Mother.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Mother.Facade.Agents;

public sealed class MotherCoordinator(
    IEnumerable<IAgent> agents,
    IEnumerable<IAgentCardProvider>? agentCardProviders = null,
    IKnowledgeAgentA2AClient? knowledgeAgentA2AClient = null,
    MotherA2AOptions? a2AOptions = null,
    ILogger<MotherCoordinator>? logger = null)
{
    private const string MasterDataAgentName = "MasterDataAgent";
    private const string SalesAgentName = "SalesAgent";
    private const string WarehouseAgentName = "WarehouseAgent";
    private const string KnowledgeAgentName = "KnowledgeAgent";

    private static readonly Regex DemandPattern = new(
        @"(?<quantity>\d+(?:[.,]\d+)?)\s*(?:bottles?|bottle|pz|pieces?)\s+(?:of\s+)?(?<beer>[a-zA-Z][a-zA-Z0-9\s'-]*?)(?=\s+(?:and|,)|\?|$)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IReadOnlyDictionary<string, IAgent> _agents = agents.ToDictionary(a => a.Name);
    private readonly IReadOnlyCollection<IAgentCardProvider> _agentCardProviders =
        agentCardProviders?.ToArray() ?? [];
    private readonly MotherA2AOptions _a2AOptions = a2AOptions ?? new MotherA2AOptions();
    private readonly ILogger<MotherCoordinator> _logger = logger ?? NullLogger<MotherCoordinator>.Instance;

    public IReadOnlyCollection<AgentCard> InspectAgentCards()
        => _agentCardProviders
            .Select(provider => provider.GetAgentCard())
            .ToArray();

    public bool CanCoordinate(ChatRequest request)
        => (IsWhatIf(request.Message) && ParseDemand(request.Message).Count > 0)
           || (_a2AOptions.Enabled && IsKnowledgeQuestion(request.Message));

    public async Task<ChatResponse> CoordinateAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        using var activity = MotherTelemetry.Source.StartActivity("Mother.Coordinate", ActivityKind.Internal);
        activity?.SetTag("brewup.conversation.id", request.ConversationId);
        activity?.SetTag("brewup.request.message", request.Message);

        try
        {
            if (_a2AOptions.Enabled && IsKnowledgeQuestion(request.Message))
            {
                activity?.SetTag("brewup.coordination.path", "knowledge-only");
                return await CoordinateKnowledgeOnlyAsync(request, cancellationToken);
            }

            activity?.SetTag("brewup.coordination.path", "what-if");

            var demandItems = ParseDemand(request.Message);
            activity?.SetTag("brewup.demand.count", demandItems.Count);
            activity?.AddEvent(new ActivityEvent(
                "demand.parsed",
                tags: new ActivityTagsCollection { ["brewup.demand.count"] = demandItems.Count }));

            if (demandItems.Count == 0)
            {
                activity?.SetTag("brewup.coordination.outcome", "no-demand-identified");
                return new ChatResponse("I could not identify the requested beer quantities in the scenario.", request.ConversationId);
            }

            var correlationId = Guid.CreateVersion7();
            activity?.SetTag("brewup.correlation.id", correlationId);
            ConversationRoot myConversation = new(correlationId);

            var invokedAgents = new List<string>();
            var context = new AgentContext(
                request.ConversationId ?? string.Empty,
                "Mother",
                invokedAgents.AsReadOnly(),
                new Dictionary<string, object?>());

            var masterDataResponse = await AskAsync(
                MasterDataAgentName,
                "resolve-beer-catalog",
                request.Message,
                new Dictionary<string, object?> { ["demandItems"] = demandItems },
                correlationId,
                context,
                invokedAgents,
                cancellationToken);
            myConversation.RaiseConversation(masterDataResponse);

            var resolved = masterDataResponse.GetRequired<IReadOnlyCollection<ResolvedBeerDemand>>("resolvedBeerDemand");
            activity?.SetTag("brewup.masterdata.resolved.count", resolved.Count);

            if (resolved.Count == 0)
            {
                activity?.SetTag("brewup.coordination.outcome", "masterdata-unresolved");
                return new ChatResponse(
                    "BrewUp ERP does not expose the requested product or beer information yet.",
                    request.ConversationId);
            }

            var salesResponse = await AskAsync(
                SalesAgentName,
                "interpret-demand-signal",
                request.Message,
                new Dictionary<string, object?> { ["resolvedBeerDemand"] = resolved },
                correlationId,
                context,
                invokedAgents,
                cancellationToken);

            var salesSignal = salesResponse.GetRequired<SalesDemandSignal>("salesDemandSignal");

            var warehouseResponse = await AskAsync(
                WarehouseAgentName,
                "evaluate-stock-impact",
                request.Message,
                new Dictionary<string, object?> { ["salesDemandSignal"] = salesSignal },
                correlationId,
                context,
                invokedAgents,
                cancellationToken);

            var knowledgeResponse = await AskAsync(
                KnowledgeAgentName,
                "retrieve-business-knowledge",
                request.Message,
                new Dictionary<string, object?> { ["resolvedBeerDemand"] = resolved },
                correlationId,
                context,
                invokedAgents,
                cancellationToken);

            var warehouseImpact = warehouseResponse.GetRequired<WarehouseImpact>("warehouseImpact");
            var knowledgeResult = knowledgeResponse.GetRequired<KnowledgeResult>("knowledgeResult");

            activity?.SetTag("brewup.invoked.agents", string.Join(",", invokedAgents));
            activity?.SetTag("brewup.warehouse.stock_risk", warehouseImpact.HasStockRisk);
            activity?.SetTag("brewup.warehouse.reorder_risk", warehouseImpact.HasReorderRisk);
            activity?.SetTag("brewup.coordination.outcome", "completed");

            return new ChatResponse(
                BuildBusinessAnswer(resolved, salesSignal, warehouseImpact, knowledgeResult),
                request.ConversationId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
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

        using var activity = MotherTelemetry.Source.StartActivity($"Agent.{agentName}", ActivityKind.Client);
        activity?.SetTag("brewup.agent.name", agentName);
        activity?.SetTag("brewup.agent.capability", capability);
        activity?.SetTag("brewup.correlation.id", correlationId);

        try
        {
            _logger.LogInformation(
                "Mother delegating to {AgentName} for {Capability} with correlation {CorrelationId}",
                agentName,
                capability,
                correlationId);

            invokedAgents.Add(agentName);

            var response = await agent.HandleAsync(
                new AgentRequest(capability, originalQuestion, inputs, correlationId, context),
                cancellationToken);

            activity?.SetTag("brewup.agent.response.success", response.IsSuccessful);
            activity?.SetTag("brewup.agent.response.summary", response.Summary);
            if (!response.IsSuccessful)
                activity?.SetStatus(ActivityStatusCode.Error, response.Summary);

            _logger.LogInformation(
                "Mother received {AgentName} result for {Capability}: {Summary}",
                response.AgentName,
                capability,
                response.Summary);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    private static bool IsWhatIf(string message)
        => message.Contains("what if", StringComparison.OrdinalIgnoreCase)
           || message.Contains("what happens if", StringComparison.OrdinalIgnoreCase)
           || message.Contains("impact", StringComparison.OrdinalIgnoreCase)
           || message.Contains("simulation", StringComparison.OrdinalIgnoreCase);

    private static bool IsKnowledgeQuestion(string message)
        => message.Contains("policy", StringComparison.OrdinalIgnoreCase)
           || message.Contains("procedure", StringComparison.OrdinalIgnoreCase)
           || message.Contains("documentation", StringComparison.OrdinalIgnoreCase)
           || message.Contains("documented", StringComparison.OrdinalIgnoreCase)
           || message.Contains("quality standard", StringComparison.OrdinalIgnoreCase)
           || message.Contains("how is beer produced", StringComparison.OrdinalIgnoreCase)
           || message.Contains("inventory management", StringComparison.OrdinalIgnoreCase);

    private async Task<ChatResponse> CoordinateKnowledgeOnlyAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = MotherTelemetry.Source.StartActivity("Agent.KnowledgeAgent.A2A", ActivityKind.Client);
        activity?.SetTag("brewup.agent.name", KnowledgeAgentName);
        activity?.SetTag("brewup.agent.transport", "a2a");
        activity?.SetTag("brewup.conversation.id", request.ConversationId);

        try
        {
            if (knowledgeAgentA2AClient is null)
                throw new InvalidOperationException("Knowledge A2A mode is enabled, but no KnowledgeAgent A2A client is registered.");

            var correlationId = Guid.CreateVersion7();
            activity?.SetTag("brewup.correlation.id", correlationId);

            var card = await knowledgeAgentA2AClient.GetAgentCardAsync(cancellationToken);
            activity?.SetTag("brewup.agent.card.name", card.Name);
            activity?.AddEvent(new ActivityEvent(
                "a2a.agent.discovered",
                tags: new ActivityTagsCollection { ["brewup.agent.card.name"] = card.Name }));

            _logger.LogInformation(
                "Mother discovered KnowledgeAgent {AgentName} with correlation {CorrelationId}",
                card.Name,
                correlationId);

            _logger.LogInformation(
                "Mother delegated task to KnowledgeAgent with correlation {CorrelationId}",
                correlationId);

            activity?.AddEvent(new ActivityEvent("a2a.task.submitted"));

            var result = await knowledgeAgentA2AClient.SubmitKnowledgeTaskAsync(
                request.Message,
                correlationId,
                cancellationToken);

            activity?.SetTag("brewup.knowledge.findings.count", result.Findings.Count);

            return new ChatResponse(BuildKnowledgeAnswer(result), request.ConversationId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    private static string BuildKnowledgeAnswer(KnowledgeResult knowledgeResult)
    {
        if (knowledgeResult.Findings.Count == 0)
            return "BrewUp ERP does not expose that information yet.";

        var first = knowledgeResult.Findings.First();
        return $"{first.Title}: {first.Content}";
    }

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
