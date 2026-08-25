using System.Diagnostics;
using System.Text.Json;
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
            ActivityKind.Internal);
        activity?.SetTag("gen_ai.operation.name", "invoke_agent");
        activity?.SetTag("gen_ai.agent.name", AgentName);
        activity?.SetTag("gen_ai.agent.id", "brewup.knowledge");
        activity?.SetTag("brewup.agent_run.id", request.CorrelationId);
        if (TryGetConversationId(request, out var conversationId))
            activity?.SetTag("gen_ai.conversation.id", conversationId);

        try
        {
            var result = await ExecuteCoreAsync(request, cancellationToken);
            activity?.SetTag("brewup.agent.success", result.ErrorType is null);
            activity?.SetTag("brewup.outcome", result.ErrorType is null ? "completed" : "failed");

            if (result.ErrorType is not null)
            {
                activity?.SetTag("error.type", result.ErrorType);
                activity?.SetStatus(ActivityStatusCode.Error);
            }

            return result.Response;
        }
        catch (Exception ex)
        {
            activity?.SetTag("brewup.agent.success", false);
            activity?.SetTag("brewup.outcome", "failed");
            activity?.SetTag("error.type", ex.GetType().FullName);
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            throw;
        }
    }

    private async Task<KnowledgeAgentExecutionResult> ExecuteCoreAsync(
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
                "KnowledgeAgent could not discover Knowledge MCP tools from the Knowledge MCP Server.",
                ex.GetType().FullName);
        }

        if (!tools.Any(tool => tool.Name.Equals("search_knowledge_base", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning(
                "KnowledgeAgent found no suitable Knowledge tool for correlation {CorrelationId}. Discovered tools: {Tools}",
                request.CorrelationId,
                string.Join(", ", tools.Select(tool => tool.Name)));

            return Failure(
                request,
                "KnowledgeAgent found no suitable Knowledge tool available for this request.",
                typeof(InvalidOperationException).FullName);
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
                "KnowledgeAgent failed while invoking search_knowledge_base on the Knowledge MCP Server.",
                ex.GetType().FullName);
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

        return new KnowledgeAgentExecutionResult(
            new A2ATaskResponse(
                request.TaskId,
                AgentName,
                isSuccessful,
                summary,
                result,
                request.CorrelationId),
            ErrorType: null);
    }

    private static KnowledgeAgentExecutionResult Failure(
        A2ATaskRequest request,
        string summary,
        string? errorType)
        => new(
            new A2ATaskResponse(
                request.TaskId,
                AgentName,
                false,
                summary,
                new KnowledgeResult([]),
                request.CorrelationId),
            errorType);

    private static bool TryGetConversationId(A2ATaskRequest request, out string? conversationId)
    {
        if (!request.Metadata.TryGetValue(A2ATaskRequest.ConversationIdMetadataKey, out var value))
        {
            conversationId = null;
            return false;
        }

        conversationId = value switch
        {
            string id => id,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => null
        };

        return !string.IsNullOrWhiteSpace(conversationId);
    }

    private sealed record KnowledgeAgentExecutionResult(
        A2ATaskResponse Response,
        string? ErrorType);
}
