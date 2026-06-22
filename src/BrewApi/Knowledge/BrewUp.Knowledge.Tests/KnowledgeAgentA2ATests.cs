using BrewUp.Knowledge.Agent;
using BrewUp.Knowledge.Agent.Tools;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BrewUp.Knowledge.Tests;

public sealed class KnowledgeAgentA2ATests
{
    [Fact]
    public void Agent_card_describes_brewup_knowledge_agent()
    {
        var provider = new BrewUpKnowledgeAgentCardProvider();

        var card = provider.GetAgentCard();

        Assert.Equal("BrewUp Knowledge Agent", card.Name);
        Assert.Contains("documented BrewUp business knowledge", card.Description);
        Assert.Contains(card.Capabilities, capability => capability.Name == "knowledge retrieval");
        Assert.Contains(card.Capabilities, capability => capability.Name == "documentation lookup");
        Assert.Contains(card.Capabilities, capability => capability.Name == "policy lookup");
        Assert.Contains(card.Capabilities, capability => capability.Name == "procedure lookup");
        Assert.Contains(card.Skills, skill => skill.Name == "search_knowledge");
        Assert.Contains("What is the reorder policy for IPA?", card.Examples);
    }

    [Fact]
    public async Task Executor_calls_search_knowledge_base_through_mcp()
    {
        var mcp = new RecordingMcpToolClient(
        [
            new McpToolMetadata(
                "search_knowledge_base",
                "Search BrewUp business knowledge.",
                null)
        ]);
        var executor = CreateExecutor(mcp);
        var correlationId = Guid.CreateVersion7();

        var response = await executor.ExecuteAsync(
            new A2ATaskRequest(
                "task-1",
                "What is the reorder policy for IPA?",
                correlationId,
                new Dictionary<string, object?>()),
            CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.Equal("BrewUp Knowledge Agent", response.AgentName);
        Assert.Contains("IPA reorder policy", response.Summary);
        Assert.Single(response.KnowledgeResult.Findings);
        Assert.Equal(["knowledge"], mcp.ListedServers);
        Assert.Contains(mcp.Calls, call => call.ServerName == "knowledge"
                                           && call.ToolName == "search_knowledge_base"
                                           && call.CorrelationId == correlationId);
    }

    [Fact]
    public async Task Executor_does_not_require_mother_to_know_knowledge_tool_names()
    {
        var mcp = new RecordingMcpToolClient(
        [
            new McpToolMetadata(
                "search_knowledge_base",
                "Search BrewUp business knowledge.",
                null)
        ]);
        var executor = CreateExecutor(mcp);

        var response = await executor.ExecuteAsync(
            new A2ATaskRequest(
                "task-no-tool-name",
                "What is the reorder policy for IPA?",
                Guid.CreateVersion7(),
                new Dictionary<string, object?>
                {
                    ["motherIntent"] = "retrieve-business-knowledge"
                }),
            CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.Contains(mcp.Calls, call => call.ToolName == "search_knowledge_base");
    }

    [Fact]
    public async Task Executor_returns_clear_failure_when_search_knowledge_base_is_missing()
    {
        var mcp = new RecordingMcpToolClient(
        [
            new McpToolMetadata(
                "some_other_knowledge_tool",
                "A different Knowledge MCP tool.",
                null)
        ]);
        var executor = CreateExecutor(mcp);

        var response = await executor.ExecuteAsync(
            new A2ATaskRequest(
                "task-missing-tool",
                "What is the reorder policy for IPA?",
                Guid.CreateVersion7(),
                new Dictionary<string, object?>()),
            CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Contains("no suitable Knowledge tool", response.Summary);
        Assert.Empty(response.KnowledgeResult.Findings);
        Assert.Empty(mcp.Calls);
        Assert.Equal(["knowledge"], mcp.ListedServers);
    }

    [Fact]
    public async Task Executor_returns_structured_failure_when_tools_list_fails()
    {
        var mcp = new RecordingMcpToolClient([], throwOnList: true);
        var executor = CreateExecutor(mcp);

        var response = await executor.ExecuteAsync(
            new A2ATaskRequest(
                "task-list-fails",
                "What is the reorder policy for IPA?",
                Guid.CreateVersion7(),
                new Dictionary<string, object?>()),
            CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Contains("could not discover Knowledge MCP tools", response.Summary);
        Assert.Empty(response.KnowledgeResult.Findings);
        Assert.Empty(mcp.Calls);
    }

    private static KnowledgeAgentExecutor CreateExecutor(RecordingMcpToolClient mcp)
    {
        var invoker = new KnowledgeAgentToolInvoker(
            mcp,
            Options.Create(new KnowledgeAgentMcpOptions()),
            NullLogger<KnowledgeAgentToolInvoker>.Instance);

        return new KnowledgeAgentExecutor(invoker);
    }

    private sealed class RecordingMcpToolClient(
        IReadOnlyCollection<McpToolMetadata>? tools = null,
        bool throwOnList = false) : IMcpToolClient
    {
        private readonly List<McpCall> _calls = [];
        private readonly List<string> _listedServers = [];

        public IReadOnlyCollection<McpCall> Calls => _calls;
        public IReadOnlyCollection<string> ListedServers => _listedServers;

        public Task<IReadOnlyCollection<McpToolMetadata>> ListToolsAsync(
            string serverName,
            CancellationToken cancellationToken)
        {
            _listedServers.Add(serverName);

            if (throwOnList)
                throw new InvalidOperationException("tools/list failed");

            return Task.FromResult(tools ?? []);
        }

        public Task<TResponse?> CallToolAsync<TResponse>(
            string serverName,
            string toolName,
            object arguments,
            CancellationToken cancellationToken)
        {
            var correlationId = (Guid)arguments.GetType().GetProperty("correlationId")!.GetValue(arguments)!;
            _calls.Add(new McpCall(serverName, toolName, correlationId));

            object? response = new SearchKnowledgeResult(
            [
                new KnowledgeSearchResultItem(
                    Guid.CreateVersion7(),
                    Guid.CreateVersion7(),
                    1,
                    "IPA reorder policy",
                    "Warehouse",
                    ["ipa", "reorder"],
                    "IPA reorder policy: review replenishment when projected stock reaches the threshold.",
                    0.91,
                    12)
            ]);

            return Task.FromResult((TResponse?)response);
        }
    }

    private sealed record McpCall(string ServerName, string ToolName, Guid CorrelationId);
}
