using BrewUp.MasterData.ReadModel.Services;
using BrewUp.Shared.ExternalContracts.MasterData;
using BrewUp.Shared.ExternalContracts.MasterData.Beers;
using BrewUp.Shared.ExternalContracts.MasterData.Customers;
using BrewUp.Shared.ExternalContracts.MasterData.Suppliers;
using BrewUp.Shared.ExternalContracts.Warehouse;

namespace BrewUp.MasterData.McpServer;

internal sealed class McpMasterDataFacade(
    IBeerQueryService beerQueryService,
    ICustomerQueryService customerQueryService,
    ISupplierQueryService supplierQueryService,
    IWarehouseQueryService warehouseQueryService) : IMcpMasterDataFacade
{
    public async Task<IReadOnlyCollection<BeerJson>> GetBeersCatalogAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var beersResult = await beerQueryService.GetBeersAsync(1, int.MaxValue, cancellationToken);
        if (beersResult.IsError)
            return [];
        
        return beersResult.TryGetValue(out var beers) 
            ? beers.Results.ToList() 
            : Array.Empty<BeerJson>();
    }

    public async Task<BeerJson> GetBeerDetailsAsync(string beerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var beerResult = await beerQueryService.GetBeerByIdAsync(beerId, cancellationToken);
        if (beerResult.IsError)
            return new BeerJson();
        
        return beerResult.TryGetValue(out var beer)
            ? beer  
            : new BeerJson();
    }

    public async Task<BeerJson> GetBeerDetailsByNameAsync(string beerName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var beersResult = await beerQueryService.GetBeersAsync(1, int.MaxValue, cancellationToken);
        if (beersResult.IsError)
            return new BeerJson();
        
        beersResult.TryGetValue(out var beers);
        
        return beers.Results.FirstOrDefault(b => b.BeerName.Contains(beerName, StringComparison.OrdinalIgnoreCase)) 
               ?? new BeerJson();
    }

    public async Task<IReadOnlyCollection<CustomerJson>> GetActiveCustomersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var customersResult = await customerQueryService.GetCustomersAsync(1, int.MaxValue, cancellationToken);
        if (customersResult.IsError)
            return [];
        
        return customersResult.TryGetValue(out var customers)
            ? customers.Results.ToList()    
            : Array.Empty<CustomerJson>();
    }

    public async Task<CustomerJson> GetCustomerInfoAsync(string customerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var customerResult = await customerQueryService.GetCustomerByIdAsync(customerId, cancellationToken);
        if (customerResult.IsError)            
            return new CustomerJson(customerId, 
                string.Empty, 
                string.Empty, 
                string.Empty, new IndirizzoJson(),
                0, false);
        
        return customerResult.TryGetValue(out var customer)
            ? customer  
            : new CustomerJson(customerId, 
                string.Empty, 
                string.Empty, 
                string.Empty, new IndirizzoJson(),
                0, false);
    }

    public async Task<IReadOnlyCollection<SupplierJson>> GetActiveSuppliersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var suppliersResult = await supplierQueryService.GetSuppliersAsync(1, int.MaxValue,  cancellationToken);
        if (suppliersResult.IsError)
            return [];
        
        return suppliersResult.TryGetValue(out var suppliers)
            ? suppliers.Results.ToList()    
            : Array.Empty<SupplierJson>();
    }

    public async Task<SupplierJson> GetSupplierInfoAsync(string supplierId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var supplierResult = await supplierQueryService.GetSupplierByIdAsync(supplierId, cancellationToken);
        if (supplierResult.IsError)            
            return new SupplierJson
            {
                SupplierId = supplierId,
                RagioneSociale = string.Empty,
                PartitaIva = string.Empty,
                Indirizzo = new IndirizzoJson(),
                IsEnabled = false
            };
        
        return supplierResult.TryGetValue(out var supplier)
            ? supplier  
            : new SupplierJson
            {
                SupplierId = supplierId,
                RagioneSociale = string.Empty,
                PartitaIva = string.Empty,
                Indirizzo = new IndirizzoJson(),
                IsEnabled = false
            };
    }

    public async Task<IReadOnlyCollection<WarehouseJson>> GetActiveWarehousesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var warehousesResult = await warehouseQueryService.GetWarehousesAsync(1, int.MaxValue,  cancellationToken);
        if (warehousesResult.IsError)
            return [];
        
        return warehousesResult.TryGetValue(out var suppliers)
            ? suppliers.Results.ToList()    
            : Array.Empty<WarehouseJson>();
    }
}