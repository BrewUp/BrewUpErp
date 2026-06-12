using System.ComponentModel;
using BrewUp.Knowledge.SharedKernel.Documents;
using ModelContextProtocol.Server;

namespace BrewUp.Knowledge.McpServer.Tools;

[McpServerToolType]
public sealed class KnowledgeTools(IKnowledgeFacade knowledgeFacade)
{
    [McpServerTool(Name = "search_knowledge_base")]
    [Description("Search BrewUp business knowledge, documentation, procedures, policies, and general domain information.")]
    public async Task<object> SearchKnowledgeBaseAsync(
        [Description("The natural language question or search query.")]
        string query,

        [Description("Optional bounded context scope. Allowed values: General, Sales, Warehouse, MasterData, Production.")]
        string? scope,

        [Description("Maximum number of chunks to retrieve. Default is 5.")]
        int topK,

        CancellationToken cancellationToken)
    {
        var request = new SearchKnowledgeBaseRequest(
            Query: query,
            Scope: scope,
            TopK: topK <= 0 ? 5 : Math.Min(topK, 20));

        return await knowledgeFacade.SearchKnowledgeBaseAsync(
            request,
            cancellationToken);
    }
}