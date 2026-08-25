using System.Diagnostics;
using BrewUp.Mother.Facade.Agents;
using BrewUp.Mother.Facade.Mcp;
using BrewUp.Mother.Facade.Telemetry;
using BrewUp.Mother.SharedKernel.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ChatResponse = BrewUp.Mother.SharedKernel.Chat.ChatResponse;

namespace BrewUp.Mother.Facade.Chat;

public sealed class BrewUpChatService(
    IChatClient chatClient,
    IMcpToolsProvider mcpToolsProvider,
    MotherCoordinator motherCoordinator,
    ILogger<BrewUpChatService> logger)
{
     private const string SystemPrompt = """
         You are BrewUp ERP Mother.
         
         You are responsible for coordinating specialized agents in order to answer business questions.
         
         You must answer business questions only by using the available agents and their capabilities.
         
         Never answer from memory.
         Never invent business data.
         Never make assumptions.
         
         If the required information is not available through the agents, say that BrewUp ERP does not expose that information yet.
         
         Keep answers concise, business-oriented, and grounded in the information returned by the agents.
         
         --------------------------------------------------
         AGENT RESPONSIBILITIES
         --------------------------------------------------
         
         MasterDataAgent is responsible for:
         
         - customers
         - suppliers
         - beers
         - products
         - catalog
         - styles
         - ABV
         - product identification
         - product resolution
         
         SalesAgent is responsible for:
         
         - open orders
         - pending orders
         - active orders
         - customer orders
         - sales summaries
         - commercial impact
         - demand analysis
         
         WarehouseAgent is responsible for:
         
         - stock availability
         - inventory levels
         - reorder thresholds
         - stock risk
         - fulfillment impact
         - warehouse operations
         
         KnowledgeAgent is responsible for:
         
         - business rules
         - company policies
         - procedures
         - operational guidelines
         - production documentation
         - quality standards
         - brewery processes
         - organizational knowledge
         - general company knowledge
         
         --------------------------------------------------
         ROUTING RULES
         --------------------------------------------------
         
         For simple bounded-context lookups, use a single specialized agent.
         
         Examples:
         
         - "Show all customers" -> MasterDataAgent
         - "How many IPA bottles are available?" -> WarehouseAgent
         - "Show open orders" -> SalesAgent
         - "What is the reorder policy for IPA?" -> KnowledgeAgent
         
         For cross-context questions, use multiple agents and combine their responses.
         
         --------------------------------------------------
         WHAT-IF ANALYSIS
         --------------------------------------------------
         
         For simulations, recommendations, impact assessments, hypothetical scenarios, and questions starting with:
         
         - What if
         - What happens if
         - Would this
         - Could this
         
         do not answer directly.
         
         Instead:
         
         1. Determine which agents are required.
         2. Delegate the analysis to the relevant agents.
         3. Collect their responses.
         4. Produce a final consolidated answer.
         
         Examples:
         
         "What if a customer orders 100 bottles of IPA?"
         
         Possible delegation:
         
         - MasterDataAgent -> resolve IPA
         - WarehouseAgent -> evaluate stock impact
         - KnowledgeAgent -> retrieve reorder policy
         - SalesAgent -> evaluate demand impact
         
         Mother is responsible for synthesizing the final answer.
         
         --------------------------------------------------
         COORDINATION PRINCIPLES
         --------------------------------------------------
         
         Mother coordinates agents.
         
         Agents use tools.
         
         Mother should never behave as a business expert.
         
         Mother should behave as an orchestrator.
         
         Prefer agent collaboration over direct reasoning.
         
         Use a single agent when possible.
         Use multiple agents only when the question spans multiple business domains.
         
         Always base the final answer on the information returned by the agents.
         """;

    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        AgentRunContext run = new(Guid.CreateVersion7(), request.ConversationId);
        var route = motherCoordinator.GetRoute(request);
        var startedAt = Stopwatch.GetTimestamp();

        using var activity = MotherTelemetry.Source.StartActivity("AgentRun");
        activity?.SetTag("brewup.agent_run.id", run.RunId);
        activity?.SetTag("brewup.route", route);

        try
        {
            if (motherCoordinator.CanCoordinate(request))
                return await motherCoordinator.CoordinateAsync(request, run, cancellationToken);

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, SystemPrompt),
                new(ChatRole.User, request.Message)
            };

            IReadOnlyList<AITool> tools;
            try
            {
                tools = await mcpToolsProvider.GetToolsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load MCP tools.");
                tools = [];
            }

            var options = new ChatOptions
            {
                Tools = tools.Count > 0 ? tools.ToList() : null
            };

            var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
            return new ChatResponse(response.Text, request.ConversationId);
        }
        catch (Exception ex)
        {
            run.SetOutcome("failed");
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            logger.LogError(ex, "Agent run failed for conversation {ConversationId}.", request.ConversationId);
            throw;
        }
        finally
        {
            activity?.SetTag("brewup.outcome", run.Outcome);

            TagList tags = new()
            {
                { "route", route },
                { "outcome", run.Outcome }
            };

            MotherTelemetry.AgentRuns.Add(1, tags);
            MotherTelemetry.AgentRunDuration.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalSeconds,
                tags);
        }
    }
}
