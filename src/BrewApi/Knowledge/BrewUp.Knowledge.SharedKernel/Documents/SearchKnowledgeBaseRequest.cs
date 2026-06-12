namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed record SearchKnowledgeBaseRequest(
    string Query,
    string? Scope,
    int TopK = 5);