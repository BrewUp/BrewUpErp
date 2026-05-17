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
    
    [McpServerTool(Name = "get_beer_details")]
    [Description(
        "Returns the details of a beer. Use this tool when someone asks for beer details.")]
    public async Task<object> GetBeerDetails(
        [Description("The beer id, or part of the beer id. Use this tool when someone asks beer details.")]
        string beerId,
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetBeerDetailsAsync(beerId, cancellationToken);
    
    [McpServerTool(Name = "get_active_customers")]
    [Description("Returns the currently active customers. Use this tool when someone asks for active customers.")]
    public async Task<object> GetActiveCustomers(
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetActiveCustomersAsync(cancellationToken);
    
    [McpServerTool(Name = "get_customer_info")]
    [Description("Returns the customer details of a customer. Use this tool when someone asks for customer details.")]
    public async Task<object> GetCustomerInfo(
        [Description("The customer id, or part of the customer id.")]
        string customerId,
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetCustomerInfoAsync(customerId, cancellationToken);
}