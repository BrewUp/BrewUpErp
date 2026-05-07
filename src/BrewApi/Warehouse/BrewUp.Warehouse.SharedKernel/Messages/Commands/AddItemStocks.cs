using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Warehouse;
using Muflone.Messages.Commands;

namespace BrewUp.Warehouse.SharedKernel.Messages.Commands
{
    public sealed class AddItemStocks(WarehouseId aggregateId, IEnumerable<ItemStockJson> rows, 
        Guid correlationId) : Command(aggregateId, correlationId)
    {
        public IEnumerable<ItemStockJson> Rows { get; private set; } = rows;
    }
}
