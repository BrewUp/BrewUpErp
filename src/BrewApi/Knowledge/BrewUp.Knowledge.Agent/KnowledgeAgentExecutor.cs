using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Logging.Abstractions;

namespace BrewUp.Knowledge.Agent;

public sealed class KnowledgeAgentExecutor(
    IMcpToolClient mcpToolClient,
    ILogger<KnowledgeAgentExecutor>? logger = null)
{
    public const string AgentName = "BrewUp Knowledge Agent";
    private readonly ILogger<KnowledgeAgentExecutor> _logger = logger ?? NullLogger<KnowledgeAgentExecutor>.Instance;

    public async Task<A2ATaskResponse> ExecuteAsync(
        A2ATaskRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "KnowledgeAgent executing MCP tool {ToolName} with correlation {CorrelationId}",
            "search_knowledge_base",
            request.CorrelationId);

        var searchResult = await mcpToolClient.CallToolAsync<SearchKnowledgeResult>(
            serverName: "knowledge",
            toolName: "search_knowledge_base",
            arguments: new
            {
                query = request.Message,
                scope = (string?)null,
                topK = 5,
                correlationId = request.CorrelationId
            },
            cancellationToken);

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
}
