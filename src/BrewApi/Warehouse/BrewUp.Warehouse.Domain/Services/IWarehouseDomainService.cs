using BrewUp.Shared.ExternalContracts.Warehouse;
using Lena.Core;

namespace BrewUp.Warehouse.Domain.Services
{
    public interface IWarehouseDomainService
    {
        Task<Result<string>> AddItemStocksAsync(WarehouseJson body, CancellationToken cancellationToken);
    }
}
