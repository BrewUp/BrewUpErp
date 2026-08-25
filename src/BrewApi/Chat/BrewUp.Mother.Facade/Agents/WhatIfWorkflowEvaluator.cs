using BrewUp.Shared.Agents;

namespace BrewUp.Mother.Facade.Agents;

internal static class WhatIfWorkflowEvaluator
{
    public static WhatIfWorkflowEvaluation Evaluate(
        IReadOnlyCollection<DemandItem> demandItems,
        IReadOnlyCollection<ResolvedBeerDemand> resolvedDemand,
        AgentResponse masterDataResponse,
        AgentResponse salesResponse,
        SalesDemandSignal salesSignal,
        AgentResponse warehouseResponse,
        WarehouseImpact warehouseImpact,
        AgentResponse knowledgeResponse,
        KnowledgeResult knowledgeResult)
    {
        var requiredAgentResultsObtained =
            masterDataResponse.IsSuccessful
            && salesResponse.IsSuccessful
            && warehouseResponse.IsSuccessful
            && knowledgeResponse.IsSuccessful;

        var requestedProductsResolved =
            demandItems.Count > 0
            && resolvedDemand.Count == demandItems.Count
            && resolvedDemand.All(item => !string.IsNullOrWhiteSpace(item.Beer.BeerId));

        return new WhatIfWorkflowEvaluation(
            requiredAgentResultsObtained,
            requestedProductsResolved,
            salesSignal.Lines.Count > 0,
            warehouseImpact.Lines.Count >= resolvedDemand.Count,
            knowledgeResult.Findings.Count > 0);
    }
}

internal sealed record WhatIfWorkflowEvaluation(
    bool RequiredAgentResultsObtained,
    bool RequestedProductsResolved,
    bool SalesEvidenceAvailable,
    bool WarehouseEvidenceAvailable,
    bool KnowledgeEvidenceAvailable)
{
    public bool Passed =>
        RequiredAgentResultsObtained
        && RequestedProductsResolved
        && SalesEvidenceAvailable
        && WarehouseEvidenceAvailable
        && KnowledgeEvidenceAvailable;
}
