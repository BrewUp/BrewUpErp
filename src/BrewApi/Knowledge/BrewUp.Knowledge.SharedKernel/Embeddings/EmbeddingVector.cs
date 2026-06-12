namespace BrewUp.Knowledge.SharedKernel.Embeddings;

public sealed class EmbeddingVector(IEnumerable<float> values)
{
    public IReadOnlyList<float> Values { get; } = values.ToArray();

    public int Dimensions => Values.Count;
}
