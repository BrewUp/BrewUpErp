namespace BrewUp.Knowledge.SharedKernel.Embeddings;

public interface IEmbeddingGenerator
{
    Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken);
}