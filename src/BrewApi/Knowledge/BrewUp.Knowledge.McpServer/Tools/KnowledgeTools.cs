using System.ComponentModel;
using BrewUp.Knowledge.SharedKernel.Documents;
using BrewUp.Knowledge.SharedKernel.Wiki;
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

    [McpServerTool(Name = "query_wiki")]
    [Description("Search synthesized, comparatively stable BrewUp domain knowledge. Wiki results are derived interpretations, not authoritative operational ERP state.")]
    public Task<object> QueryWikiAsync(
        [Description("The natural language concept, policy, procedure, or terminology to find.")]
        string query,
        [Description("Optional bounded context scope.")]
        string? scope,
        [Description("Maximum number of Wiki pages to retrieve. Default is 5.")]
        int topK,
        CancellationToken cancellationToken)
        => knowledgeFacade.QueryWikiAsync(
            new QueryWikiRequest(query, scope, topK <= 0 ? 5 : Math.Min(topK, 20)),
            cancellationToken);

    [McpServerTool(Name = "get_wiki_page")]
    [Description("Get the current synthesized content, claims, links, and unresolved issues for a Wiki page.")]
    public Task<object?> GetWikiPageAsync(
        [Description("Stable Wiki page key or display title.")]
        string key,
        CancellationToken cancellationToken)
        => knowledgeFacade.GetWikiPageAsync(key, cancellationToken);

    [McpServerTool(Name = "get_wiki_page_evidence")]
    [Description("Get the source document chunks that support the current claims of a Wiki page.")]
    public Task<object?> GetWikiPageEvidenceAsync(
        [Description("Wiki page identifier.")]
        Guid pageId,
        CancellationToken cancellationToken)
        => knowledgeFacade.GetWikiPageEvidenceAsync(pageId, cancellationToken);
}