using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Muflone.Messages.Commands;

namespace BrewUp.Sales.SharedKernel.Messages.Commands;

public sealed class AddBeerToCart(SalesOrderId aggregateId, SalesOrderRowJson row) : Command(aggregateId)
{
    public SalesOrderRowJson Row { get; private set; } = row;
}