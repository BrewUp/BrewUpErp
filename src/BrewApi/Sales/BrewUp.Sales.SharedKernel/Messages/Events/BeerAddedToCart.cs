using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Messages.Events;

namespace BrewUp.Sales.SharedKernel.Messages.Events;

public sealed class BeerAddedToCart(SalesOrderId aggregateId, SalesOrderRowJson row) : DomainEvent(aggregateId)
{
    public SalesOrderRowJson Row { get; private set; } = row;
}