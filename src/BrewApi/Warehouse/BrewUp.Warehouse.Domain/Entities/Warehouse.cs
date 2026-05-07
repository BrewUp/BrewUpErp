using BrewUp.Shared.DomainIds;
using BrewUp.Warehouse.Entities.Dtos;
using Muflone.Core;

namespace BrewUp.Warehouse.Domain.Entities
{
    public class Warehouse : AggregateRoot
    {
        internal string Name { get; } = string.Empty;
        internal Dictionary<BeerId, ItemStock> ItemStocks { get; } = new Dictionary<BeerId, ItemStock>();

        protected Warehouse() { }

        internal static Warehouse Create(WarehouseId aggregateId, string name, IEnumerable<ItemStockJson> rows, Guid correlationId)
        {
            return new Warehouse(aggregateId, name, rows, correlationId);
        }

        private Warehouse(WarehouseId aggregateId, string name, IEnumerable<ItemStockJson> rows, Guid correlationId)
        {
            Name = name;
            foreach (var row in rows)
            {
                ItemStocks.Add(new BeerId(row.BeerId), ItemStock.Create(new BeerId(row.BeerId), decimal.Parse(row.Quantity)));
            }
        }
    }
}
