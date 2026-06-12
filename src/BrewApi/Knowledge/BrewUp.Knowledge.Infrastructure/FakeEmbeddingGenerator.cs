using System.Security.Cryptography;
using System.Text;
using BrewUp.Knowledge.SharedKernel.Embeddings;

namespace BrewUp.Knowledge.Infrastructure;

public sealed class FakeEmbeddingGenerator : IEmbeddingGenerator
{
    private const int Dimensions = 32;

    public Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required to generate an embedding.", nameof(text));

        cancellationToken.ThrowIfCancellationRequested();

        var values = new float[Dimensions];
        var words = text.Split(
            [' ', '\r', '\n', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var word in words)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(word.ToLowerInvariant()));
            var index = BitConverter.ToUInt16(hash, 0) % Dimensions;
            values[index] += (hash[2] & 1) == 0 ? 1f : -1f;
        }

        var magnitude = MathF.Sqrt(values.Sum(value => value * value));
        if (magnitude > 0)
        {
            for (var index = 0; index < values.Length; index++)
                values[index] /= magnitude;
        }

        return Task.FromResult(new EmbeddingVector(values));
    }
}
