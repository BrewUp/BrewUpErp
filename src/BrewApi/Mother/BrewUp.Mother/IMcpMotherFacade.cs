using BrewUp.Mother.Contracts;

namespace BrewUp.Mother;

public interface IMcpMotherFacade
{
    Task<WhatIfInventoryImpactResult> AnalyzeInventoryImpactAsync(
        WhatIfInventoryImpactRequest whatIfInventoryImpactRequest, CancellationToken cancellationToken);
}