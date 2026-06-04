namespace BrewUp.Knowledge.Core.Embeddings;

public interface IEmbeddingGenerator
{
    Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken);
}