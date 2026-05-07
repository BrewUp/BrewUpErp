using Muflone.Core;

namespace BrewUp.Warehouse.SharedKernel.CustomTypes
{
    public sealed class ItemStockId(string value) : DomainId(value);
}
