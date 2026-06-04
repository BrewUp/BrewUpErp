namespace BrewUp.Knowledge.McpServer;

public interface IKnowledgeFacade
{
    Task<object> SearchKnowledgeBaseAsync(CancellationToken cancellationToken);
}