using BrewUp.Shared.ExternalContracts.Warehouse;
using Lena.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrewUp.Warehouse.Domain.Services
{
    internal class WarehouseDomainService : IWarehouseDomainService
    {
        public Task<Result<string>> AddItemStocksAsync(WarehouseJson body, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
