namespace BrewUp.Knowledge.SharedKernel.Documents;

public sealed record KnowledgeRetrievalEvaluationResult(
    string Question,
    bool Passed,
    bool ExpectedDocumentFound,
    IReadOnlyCollection<string> MatchedContentTerms,
    IReadOnlyCollection<string> MissingContentTerms,
    IReadOnlyCollection<KnowledgeSearchResultItem> RetrievedItems,
    string Diagnostic);
