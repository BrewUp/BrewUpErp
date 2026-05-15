using System.ComponentModel;
using ModelContextProtocol.Server;

namespace BrewUp.Warehouse.McpServer.Tools;

[McpServerToolType]
public sealed class WarehouseTools(
 IMcpWarehouseFacade mcpWarehouseFacade)
{
/*
 * Warehouse MCP
- get_low_stock_items
- get_reserved_stock
 */
 [McpServerTool(Name = "get_beer_availability")]
 [Description("Returns the currently availability of a beer.")]
 public async Task<object> GetBeerAvailability(
  [Description("The beerId.")]
  string beerId, CancellationToken cancellationToken)
  => await mcpWarehouseFacade.GetBeerAvailabilityAsync(beerId, cancellationToken);
 
 [McpServerTool(Name = "get_reorder_thresholds")]
 [Description("Returns the reorder threshold of a beer.")]
 public async Task<object> GetReorderThreshold(
  [Description("The beerId.")]
  string beerId, CancellationToken cancellationToken)
  => await mcpWarehouseFacade.GetReorderThresholdAsync(beerId, cancellationToken);

}