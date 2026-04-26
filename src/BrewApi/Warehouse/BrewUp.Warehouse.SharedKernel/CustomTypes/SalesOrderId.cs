using Muflone.Core;

namespace BrewUp.Warehouse.SharedKernel.CustomTypes;

public sealed class SalesOrderId(string value) : DomainId(value)
{
    public static SalesOrderId New() => new(Guid.NewGuid().ToString());
}