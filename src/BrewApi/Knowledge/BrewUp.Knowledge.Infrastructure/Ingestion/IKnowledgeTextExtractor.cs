namespace BrewUp.Knowledge.Infrastructure.Ingestion;

public interface IKnowledgeTextExtractor
{
    bool CanExtract(string fileExtension);

    Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken);
}
