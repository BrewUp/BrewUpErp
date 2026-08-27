using BrewUp.Knowledge.SharedKernel.Wiki;

namespace BrewUp.Knowledge.SharedKernel.CustomTypes;

public sealed record IngestKnowledgeDocumentResult(
    Guid DocumentId,
    int ChunkCount,
    WikiProcessingStatus WikiStatus = WikiProcessingStatus.Disabled);
