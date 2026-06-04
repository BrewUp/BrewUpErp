namespace BrewUp.Knowledge.Core.Search;

public interface IKnowledgeSearchEngine
{
    Task<KnowledgeSearchResult> SearchAsync(KnowledgeSearchRequest request, CancellationToken cancellationToken);
}