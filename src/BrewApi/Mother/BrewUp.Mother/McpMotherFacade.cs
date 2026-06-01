using BrewUp.Mother.Contracts;
using BrewUp.Mother.Mcp;

namespace BrewUp.Mother;

internal sealed class McpMotherFacade(
    IMcpToolsProvider mcpToolsProvider,
    ILoggerFactory loggerFactory) : IMcpMotherFacade
{
    private const string MasterDataServer = "MasterData";
    private const string WarehouseServer = "Warehouse";

    private readonly ILogger<McpMotherFacade> _logger = loggerFactory.CreateLogger<McpMotherFacade>();

    public async Task<WhatIfInventoryImpactResult> AnalyzeInventoryImpactAsync(
        WhatIfInventoryImpactRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Quantity <= 0)
        {
            return new WhatIfInventoryImpactResult(
                MotherAnalysisStatus.NeedsClarification,
                "The requested quantity must be greater than zero.",
                null,
                null,
                request.Quantity,
                [],
                "Please specify a positive quantity.");
        }

        BeerResolutionResult? beer;
        try
        {
            beer = await mcpToolsProvider.CallToolAsync<BeerResolutionResult>(
                MasterDataServer,
                "masterdata_resolve_beer",
                new { beerReference = request.BeerReference },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call MasterData MCP tool 'masterdata_resolve_beer'.");
            return new WhatIfInventoryImpactResult(
                MotherAnalysisStatus.NotEnoughData,
                "MasterData is currently unavailable.",
                null, null, request.Quantity, [],
                "Retry later or check the MasterData MCP server.");
        }

        if (beer is null || !beer.Found)
        {
            return new WhatIfInventoryImpactResult(
                MotherAnalysisStatus.NeedsClarification,
                $"I could not identify the beer from '{request.BeerReference}'.",
                null,
                null,
                request.Quantity,
                [],
                "Ask the user to clarify which beer they mean.");
        }

        WarehouseAvailability? availability;
        try
        {
            availability = await mcpToolsProvider.CallToolAsync<WarehouseAvailability>(
                WarehouseServer,
                "warehouse_get_item_availability",
                new { beerId = beer.BeerId },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Warehouse MCP tool 'warehouse_get_item_availability'.");
            availability = null;
        }

        if (availability is null)
        {
            return new WhatIfInventoryImpactResult(
                MotherAnalysisStatus.NotEnoughData,
                $"I identified {beer.BeerName}, but warehouse availability is not available.",
                beer.BeerId,
                beer.BeerName,
                request.Quantity,
                [],
                "Check whether Warehouse exposes availability for this beer.");
        }

        var residualQuantity = availability.AvailableQuantity - request.Quantity;
        var belowThreshold = residualQuantity < availability.ReorderThreshold;

        var item = new InventoryImpactItem(
            BeerId: beer.BeerId!,
            BeerName: beer.BeerName!,
            RequestedQuantity: request.Quantity,
            AvailableQuantity: availability.AvailableQuantity,
            ResidualQuantity: residualQuantity,
            ReorderThreshold: availability.ReorderThreshold,
            BelowReorderThreshold: belowThreshold,
            Reason: belowThreshold
                ? "The hypothetical order would reduce stock below the reorder threshold."
                : "The hypothetical order would not reduce stock below the reorder threshold.");

        var summary = belowThreshold
            ? $"If someone orders {request.Quantity} bottles of {beer.BeerName}, warehouse stock would fall below the reorder threshold."
            : $"If someone orders {request.Quantity} bottles of {beer.BeerName}, warehouse stock would remain above the reorder threshold.";

        var recommendation = belowThreshold
            ? "Consider creating a reorder recommendation or asking Warehouse to review replenishment."
            : "No immediate replenishment action is required based on the current threshold.";

        return new WhatIfInventoryImpactResult(
            MotherAnalysisStatus.Completed,
            summary,
            beer.BeerId,
            beer.BeerName,
            request.Quantity,
            [item],
            recommendation);
    }
}