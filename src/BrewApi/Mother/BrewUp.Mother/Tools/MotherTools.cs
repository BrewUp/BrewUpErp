using System.ComponentModel;
using BrewUp.Mother.Contracts;
using ModelContextProtocol.Server;

namespace BrewUp.Mother.Tools;

[McpServerToolType]
public sealed class MotherTools(
    IMcpMotherFacade motherFacade)
{
    [McpServerTool(Name = "mother_what_if_inventory_impact")]
    [Description("""
                 Use this tool for what-if questions about the inventory impact of a hypothetical order.
                 Example: "What happens to the warehouse if someone orders 100 bottles of IPA?"
                 """)]
    public Task<WhatIfInventoryImpactResult> WhatIfInventoryImpact(
        string beerReference,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        return motherFacade.AnalyzeInventoryImpactAsync(
            new WhatIfInventoryImpactRequest(
                BeerReference: beerReference,
                Quantity: quantity, 
                OriginalQuestion: beerReference),
            cancellationToken);
    }
}