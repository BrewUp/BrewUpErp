using System.ComponentModel;
using ModelContextProtocol.Server;

namespace BrewUp.Warehouse.McpServer.Tools;

[McpServerToolType]
public class WarehouseTools(IMcpWarehouseFacade mcpWarehouseFacade)
{
    [McpServerTool(Name = "get_beer_availability")]
    [Description(
        "Returns the currently availability of a beer. Use this tool when someone asks for beer availability.")]
    public async Task<object> GetBeerAvailability(
        [Description("The beer id, or part of the beer id.")]
        string beerId,
        CancellationToken cancellationToken) =>
        await mcpWarehouseFacade.GetBeerAvailabilityAsync(beerId, cancellationToken);
    
    [McpServerTool(Name = "get_reorder_thresholds")]
    [Description("Returns the reorder threshold of a beer. Use this tool to discover the reorder threshold.")]
    public async Task<object> GetReorderThreshold(
        [Description("The beer id, or part of the beer id.")]
        string beerId,
        CancellationToken cancellationToken) =>
        await mcpWarehouseFacade.GetReorderThresholdAsync(beerId, cancellationToken);
}