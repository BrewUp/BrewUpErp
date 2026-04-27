using Muflone.Core;

namespace BrewUp.Shared.DomainIds;

public sealed class SalesOrderId(string value) : DomainId(value)
{
    public static SalesOrderId New() => new(Guid.NewGuid().ToString());
}