using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Shared.Agents;

namespace BrewUp.Knowledge.Agent;

public interface IKnowledgeAgentToolInvoker
{
    Task<IReadOnlyCollection<McpToolMetadata>> DiscoverToolsAsync(CancellationToken cancellationToken);

    Task<SearchKnowledgeResult?> SearchKnowledgeBaseAsync(
        string query,
        string? scope,
        Guid correlationId,
        CancellationToken cancellationToken);
}
