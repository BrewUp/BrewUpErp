using System.Diagnostics;
using BrewUp.Knowledge.Agent.Telemetry;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Options;

namespace BrewUp.Knowledge.Agent.Tools;

public sealed class KnowledgeAgentToolInvoker(
    IMcpToolClient mcpToolClient,
    IOptions<KnowledgeAgentMcpOptions> options,
    ILogger<KnowledgeAgentToolInvoker> logger) : IKnowledgeAgentToolInvoker
{
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private IReadOnlyCollection<McpToolMetadata>? _discoveredTools;

    public async Task<IReadOnlyCollection<McpToolMetadata>> DiscoverToolsAsync(CancellationToken cancellationToken)
    {
        if (_discoveredTools is not null)
            return _discoveredTools;

        await _discoveryLock.WaitAsync(cancellationToken);
        try
        {
            if (_discoveredTools is not null)
                return _discoveredTools;

            var serverName = options.Value.ServerName;

            logger.LogInformation(
                "KnowledgeAgent connecting to Knowledge MCP Server {ServerName}",
                serverName);

            _discoveredTools = await mcpToolClient.ListToolsAsync(
                serverName,
                cancellationToken);

            logger.LogInformation(
                "KnowledgeAgent discovered Knowledge MCP tools from {ServerName}: {Tools}",
                serverName,
                string.Join(", ", _discoveredTools.Select(tool => tool.Name)));

            return _discoveredTools;
        }
        finally
        {
            _discoveryLock.Release();
        }
    }

    public async Task<SearchKnowledgeResult?> SearchKnowledgeBaseAsync(
        string query,
        string? scope,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var serverName = options.Value.ServerName;

        logger.LogInformation(
            "KnowledgeAgent invoked search_knowledge_base through {ServerName} with correlation {CorrelationId}",
            serverName,
            correlationId);

        using var activity = KnowledgeAgentTelemetry.Source.StartActivity(
            "execute_tool search_knowledge_base",
            ActivityKind.Client);
        activity?.SetTag("gen_ai.operation.name", "execute_tool");
        activity?.SetTag("gen_ai.tool.name", "search_knowledge_base");
        activity?.SetTag("brewup.agent_run.id", correlationId);

        try
        {
            var result = await mcpToolClient.CallToolAsync<SearchKnowledgeResult>(
                serverName,
                "search_knowledge_base",
                new
                {
                    query,
                    scope,
                    topK = options.Value.DefaultTopK <= 0 ? 5 : options.Value.DefaultTopK,
                    correlationId
                },
                cancellationToken);

            activity?.SetTag("brewup.outcome", "completed");
            RecordToolCall("completed");
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("brewup.outcome", "failed");
            activity?.SetStatus(ActivityStatusCode.Error);
            activity?.AddException(ex);
            RecordToolCall("failed");
            throw;
        }
    }

    private static void RecordToolCall(string outcome)
    {
        TagList tags = new()
        {
            { "agent", KnowledgeAgentExecutor.AgentName },
            { "tool", "search_knowledge_base" },
            { "outcome", outcome }
        };

        KnowledgeAgentTelemetry.ToolCalls.Add(1, tags);
    }
}
