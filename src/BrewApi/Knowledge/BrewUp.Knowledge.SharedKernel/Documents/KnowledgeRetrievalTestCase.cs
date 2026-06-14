namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed record KnowledgeRetrievalTestCase(
    string Question,
    string? ExpectedDocumentTitle,
    string? ExpectedScope,
    IReadOnlyCollection<string> ExpectedContentTerms,
    int? TopK = null);
