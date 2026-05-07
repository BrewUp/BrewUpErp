using BrewUp.Shared.DomainIds;
using BrewUp.Warehouse.SharedKernel.CustomTypes;
using Muflone.Messages.Commands;

namespace BrewUp.Warehouse.SharedKernel.Messages.Commands
{
    public sealed class RequestBeersAvailability(WarehouseId warehouseId,
                                    Guid correlationId,
                                    IEnumerable<ItemRequest> rows) : Command(warehouseId, correlationId)
    {
        public IEnumerable<ItemRequest> Rows { get; } = rows;
    }
}
