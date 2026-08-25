using System.Diagnostics;
using BrewUp.Knowledge.Agent.Telemetry;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Logging.Abstractions;

namespace BrewUp.Knowledge.Agent.Tools;

public sealed class KnowledgeAgentExecutor(
    IKnowledgeAgentToolInvoker toolInvoker,
    ILogger<KnowledgeAgentExecutor>? logger = null)
{
    public const string AgentName = "BrewUp Knowledge Agent";
    private readonly ILogger<KnowledgeAgentExecutor> _logger = logger ?? NullLogger<KnowledgeAgentExecutor>.Instance;

    public async Task<A2ATaskResponse> ExecuteAsync(
        A2ATaskRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = KnowledgeAgentTelemetry.Source.StartActivity(
            $"invoke_agent {AgentName}",
            ActivityKind.Client);
        activity?.SetTag("gen_ai.operation.name", "invoke_agent");
        activity?.SetTag("gen_ai.agent.name", AgentName);
        activity?.SetTag("gen_ai.agent.id", "brewup.knowledge");
        activity?.SetTag("brewup.agent_run.id", request.CorrelationId);

        try
        {
            var response = await ExecuteCoreAsync(request, cancellationToken);
            var outcome = response.IsSuccessful ? "completed" : "failed";
            activity?.SetTag("brewup.agent.success", response.IsSuccessful);
            activity?.SetTag("brewup.outcome", outcome);

            if (!response.IsSuccessful)
                activity?.SetStatus(ActivityStatusCode.Error);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetTag("brewup.agent.success", false);
            activity?.SetTag("brewup.outcome", "failed");
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            throw;
        }
    }

    private async Task<A2ATaskResponse> ExecuteCoreAsync(
        A2ATaskRequest request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<McpToolMetadata> tools;
        try
        {
            tools = await toolInvoker.DiscoverToolsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            _logger.LogError(
                ex,
                "KnowledgeAgent could not discover Knowledge MCP tools with correlation {CorrelationId}",
                request.CorrelationId);

            return Failure(
                request,
                "KnowledgeAgent could not discover Knowledge MCP tools from the Knowledge MCP Server.");
        }

        if (!tools.Any(tool => tool.Name.Equals("search_knowledge_base", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning(
                "KnowledgeAgent found no suitable Knowledge tool for correlation {CorrelationId}. Discovered tools: {Tools}",
                request.CorrelationId,
                string.Join(", ", tools.Select(tool => tool.Name)));

            return Failure(
                request,
                "KnowledgeAgent found no suitable Knowledge tool available for this request.");
        }

        _logger.LogInformation(
            "KnowledgeAgent selected tool {ToolName} with correlation {CorrelationId}",
            "search_knowledge_base",
            request.CorrelationId);

        SearchKnowledgeResult? searchResult;
        try
        {
            searchResult = await toolInvoker.SearchKnowledgeBaseAsync(
                request.Message,
                scope: null,
                request.CorrelationId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Activity.Current?.AddException(ex);
            _logger.LogError(
                ex,
                "KnowledgeAgent MCP call failed for tool {ToolName} with correlation {CorrelationId}",
                "search_knowledge_base",
                request.CorrelationId);

            return Failure(
                request,
                "KnowledgeAgent failed while invoking search_knowledge_base on the Knowledge MCP Server.");
        }

        _logger.LogInformation(
            "KnowledgeAgent received result from search_knowledge_base with correlation {CorrelationId}",
            request.CorrelationId);

        var findings = searchResult?.Items
            .Select(item => new KnowledgeFinding(
                item.DocumentTitle,
                item.Scope,
                item.Content,
                item.Score))
            .ToArray()
            ?? [];

        var result = new KnowledgeResult(findings);
        var isSuccessful = findings.Length > 0;
        var summary = isSuccessful
            ? $"Retrieved {findings.Length} documented knowledge item(s). {findings[0].Title}: {findings[0].Content}"
            : "BrewUp ERP does not expose documented business knowledge for this request yet.";

        _logger.LogInformation(
            "KnowledgeAgent returned result for correlation {CorrelationId} with success {AgentSuccess}",
            request.CorrelationId,
            isSuccessful);

        return new A2ATaskResponse(
            request.TaskId,
            AgentName,
            isSuccessful,
            summary,
            result,
            request.CorrelationId);
    }

    private static A2ATaskResponse Failure(A2ATaskRequest request, string summary)
        => new(
            request.TaskId,
            AgentName,
            false,
            summary,
            new KnowledgeResult([]),
            request.CorrelationId);
}
