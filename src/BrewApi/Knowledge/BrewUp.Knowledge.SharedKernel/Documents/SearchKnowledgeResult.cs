namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed record SearchKnowledgeResult(
    IReadOnlyCollection<KnowledgeSearchResultItem> Items);