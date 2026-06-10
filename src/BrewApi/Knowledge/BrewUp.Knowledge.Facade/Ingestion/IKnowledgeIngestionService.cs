namespace BrewUp.Knowledge.Facade.Ingestion;

public interface IKnowledgeIngestionService
{
    Task<IngestKnowledgeDocumentResult> IngestAsync(
        IngestKnowledgeDocumentRequest request,
        CancellationToken cancellationToken);
}
