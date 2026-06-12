using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using BrewUp.Knowledge.SharedKernel.Embeddings;
using OpenAI.Embeddings;

namespace BrewUp.Knowledge.Infrastructure;

internal sealed class AzureOpenAiEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly EmbeddingClient _client;

    public AzureOpenAiEmbeddingGenerator(AzureOpenAiEmbeddingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new InvalidOperationException($"{AzureOpenAiEmbeddingOptions.SectionName}:Endpoint is required.");

        if (string.IsNullOrWhiteSpace(options.DeploymentName))
            throw new InvalidOperationException($"{AzureOpenAiEmbeddingOptions.SectionName}:DeploymentName is required.");

        AzureOpenAIClient azureClient;
        if (options.UseManagedIdentity)
        {
            TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                TenantId = options.TenantId
            });
            azureClient = new AzureOpenAIClient(new Uri(options.Endpoint), credential);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new InvalidOperationException(
                    $"{AzureOpenAiEmbeddingOptions.SectionName}:ApiKey is required when managed identity is disabled.");

            azureClient = new AzureOpenAIClient(
                new Uri(options.Endpoint),
                new ApiKeyCredential(options.ApiKey));
        }

        _client = azureClient.GetEmbeddingClient(options.DeploymentName);
    }

    public async Task<EmbeddingVector> GenerateAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required to generate an embedding.", nameof(text));

        var response = await _client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return new EmbeddingVector(response.Value.ToFloats().ToArray());
    }
}
