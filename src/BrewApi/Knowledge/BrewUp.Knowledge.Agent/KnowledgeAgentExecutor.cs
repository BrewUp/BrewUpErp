using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Logging.Abstractions;

namespace BrewUp.Knowledge.Agent;

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
        IReadOnlyCollection<McpToolMetadata> tools;
        try
        {
            tools = await toolInvoker.DiscoverToolsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
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
            "KnowledgeAgent returned result for correlation {CorrelationId}: {Summary}",
            request.CorrelationId,
            summary);

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
