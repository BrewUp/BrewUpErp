using System.ComponentModel;
using ModelContextProtocol.Server;

namespace BrewUp.MasterData.McpServer.Tools;

[McpServerToolType]
public class MasterDataTools(IMcpMasterDataFacade mcpMasterDataFacade)
{
    [McpServerTool(Name = "get_catalog_beers")]
    [Description(
        "Returns the currently catalog of beers. Use this tool when someone asks for beers' catalog.")]
    public async Task<object> GetBeersCatalog(
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetBeersCatalogAsync(cancellationToken);
}