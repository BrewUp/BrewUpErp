namespace BrewUp.Knowledge.Core.Embeddings;

public sealed class EmbeddingVector(IEnumerable<float> values)
{
    public IReadOnlyList<float> Values { get; } = values.ToArray();
}