using BrewUp.Knowledge.ReadModel.Queries;
using BrewUp.Knowledge.ReadModel.QueryHandlers;
using BrewUp.Knowledge.SharedKernel.Documents;

namespace BrewUp.Knowledge.Facade.Evaluation;

public sealed class KnowledgeRetrievalEvaluator(
    SearchKnowledgeHandler searchKnowledgeHandler)
{
    public async Task<KnowledgeRetrievalEvaluationResult> EvaluateAsync(
        KnowledgeRetrievalTestCase testCase,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(testCase);

        if (string.IsNullOrWhiteSpace(testCase.Question))
            throw new ArgumentException(
                "An evaluation question is required.",
                nameof(testCase.Question));

        var expectedTerms = testCase.ExpectedContentTerms
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Select(term => term.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var searchResult = await searchKnowledgeHandler.HandleAsync(
            new SearchKnowledgeQuery(
                testCase.Question,
                testCase.ExpectedScope,
                testCase.TopK),
            cancellationToken);

        var expectedDocumentFound =
            string.IsNullOrWhiteSpace(testCase.ExpectedDocumentTitle) ||
            searchResult.Items.Any(item => string.Equals(
                item.DocumentTitle,
                testCase.ExpectedDocumentTitle.Trim(),
                StringComparison.OrdinalIgnoreCase));

        var candidateItems = string.IsNullOrWhiteSpace(testCase.ExpectedDocumentTitle)
            ? searchResult.Items
            : searchResult.Items
                .Where(item => string.Equals(
                    item.DocumentTitle,
                    testCase.ExpectedDocumentTitle.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();

        var matchedTerms = expectedTerms
            .Where(term => candidateItems.Any(item =>
                item.Content.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var missingTerms = expectedTerms
            .Except(matchedTerms, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var passed = expectedDocumentFound && missingTerms.Length == 0;
        var diagnostic = passed
            ? $"Passed: {matchedTerms.Length} expected content term(s) were retrieved."
            : $"Failed: expected document found={expectedDocumentFound}; " +
              $"missing terms={string.Join(", ", missingTerms)}.";

        return new KnowledgeRetrievalEvaluationResult(
            testCase.Question,
            passed,
            expectedDocumentFound,
            matchedTerms,
            missingTerms,
            searchResult.Items,
            diagnostic);
    }
}
