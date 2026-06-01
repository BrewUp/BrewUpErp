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
    
    [McpServerTool(Name = "masterdata_resolve_beer")]
    [Description(
        "Returns the details of a beer. Use this tool when someone asks for resolve beer name.")]
    public async Task<object> GetBeerDetailsFromName(
        [Description("The beer name, or part of the beer name. Use this tool when someone asks beer details from beer name.")]
        string beerName,
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetBeerDetailsByNameAsync(beerName, cancellationToken);
    
    [McpServerTool(Name = "get_active_customers")]
    [Description("Returns the currently active customers. Use this tool when someone asks for active customers.")]
    public async Task<object> GetActiveCustomers(
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetActiveCustomersAsync(cancellationToken);
    
    [McpServerTool(Name = "get_customer_info")]
    [Description("Returns the details of a customer. Use this tool when someone asks for customer details.")]
    public async Task<object> GetCustomerInfo(
        [Description("The customer id, or part of the customer id.")]
        string customerId,
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetCustomerInfoAsync(customerId, cancellationToken);
    
    [McpServerTool(Name = "get_active_suppliers")]
    [Description("Returns the currently active suppliers. Use this tool when someone asks for active suppliers.")]
    public async Task<object> GetActiveSuppliers(
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetActiveSuppliersAsync(cancellationToken);
    
    [McpServerTool(Name = "get_supplier_info")]
    [Description("Returns the details of a csupplierustomer. Use this tool when someone asks for supplier details.")]
    public async Task<object> GetSupplierInfo(
        [Description("The supplier id, or part of the supplier id.")]
        string supplierId,
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetSupplierInfoAsync(supplierId, cancellationToken);
    
    [McpServerTool(Name = "get_active_warehouses")]
    [Description("Returns the currently active warehouses. Use this tool when someone asks for active warehouses.")]
    public async Task<object> GetActiveWarehouses(
        CancellationToken cancellationToken) =>
        await mcpMasterDataFacade.GetActiveWarehousesAsync(cancellationToken);
}