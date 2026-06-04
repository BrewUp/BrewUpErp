using System.ComponentModel;
using ModelContextProtocol.Server;

namespace BrewUp.Knowledge.McpServer.Tools;

[McpServerToolType]
public class KnowledgeTools(IKnowledgeFacade knowledgeFacade)
{
    [McpServerTool(Name = "search_knowledge_base")]
    [Description("Use this tool for base knowledge about BrewUp ERP.")]
    public async Task<object> GetOpenSalesOrders(CancellationToken cancellationToken)
        => await knowledgeFacade.SearchKnowledgeBaseAsync(cancellationToken);
}