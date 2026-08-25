using BrewUp.Mother.Facade.Agents;
using BrewUp.Shared.Agents;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;

namespace BrewUp.Rest.Tests.Chat;

public sealed class WhatIfWorkflowEvaluatorTests
{
    [Fact]
    public void Passes_when_all_required_results_and_evidence_are_available()
    {
        var evaluation = Evaluate(new KnowledgeResult(
        [
            new KnowledgeFinding("Reorder policy", "inventory", "Policy content", 0.9)
        ]));

        Assert.True(evaluation.Passed);
        Assert.True(evaluation.RequiredAgentResultsObtained);
        Assert.True(evaluation.RequestedProductsResolved);
        Assert.True(evaluation.SalesEvidenceAvailable);
        Assert.True(evaluation.WarehouseEvidenceAvailable);
        Assert.True(evaluation.KnowledgeEvidenceAvailable);
    }

    [Fact]
    public void Fails_without_required_knowledge_evidence()
    {
        var evaluation = Evaluate(new KnowledgeResult([]));

        Assert.False(evaluation.Passed);
        Assert.False(evaluation.KnowledgeEvidenceAvailable);
        Assert.True(evaluation.RequiredAgentResultsObtained);
        Assert.True(evaluation.RequestedProductsResolved);
        Assert.True(evaluation.SalesEvidenceAvailable);
        Assert.True(evaluation.WarehouseEvidenceAvailable);
    }

    private static WhatIfWorkflowEvaluation Evaluate(KnowledgeResult knowledgeResult)
    {
        DemandItem[] demandItems = [new("IPA", 50, "Bottle")];
        ResolvedBeerDemand[] resolvedDemand =
        [
            new(
                demandItems[0],
                new BeerJson
                {
                    BeerId = "beer-ipa",
                    BeerName = "IPA"
                })
        ];

        var successfulResponse = new AgentResponse(
            "agent",
            "completed",
            new Dictionary<string, object?>(),
            true);

        var salesSignal = new SalesDemandSignal(
        [
            new SalesDemandLine("beer-ipa", "IPA", 50, "Bottle", 2, 100)
        ],
        50,
        100);

        var warehouseImpact = new WarehouseImpact(
        [
            new WarehouseImpactLine(
                "beer-ipa",
                "IPA",
                50,
                100,
                50,
                20,
                false,
                false,
                "Bottle")
        ],
        false,
        false);

        return WhatIfWorkflowEvaluator.Evaluate(
            demandItems,
            resolvedDemand,
            successfulResponse,
            successfulResponse,
            salesSignal,
            successfulResponse,
            warehouseImpact,
            successfulResponse,
            knowledgeResult);
    }
}
