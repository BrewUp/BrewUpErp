using BrewUp.Sales.Domain.CommandHandlers;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.ExternalContracts.Sales;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.SpecificationTests;

namespace BrewUp.Sales.Tests.Domain;

public sealed class AddBeersToCartSuccessfully : CommandSpecification<AddBeersToCart>
{
    private SalesOrderId _salesOrderId = new (Guid.CreateVersion7().ToString());
    private SalesOrderNumber _salesOrderNumber = new ("SO-1000");
    private CustomerId _customerId = new (Guid.CreateVersion7().ToString());
    private CustomerName _customerName = new ("John Doe");
    private Customer _customer;
    private SalesOrderDate _salesOrderDate = new (DateTime.UtcNow);
    private SalesOrderDeliveryDate _salesOrderDeliveryDate = new (DateTime.UtcNow.AddDays(7));
    private readonly List<SalesOrderRowJson> _rows = [];
    
    private readonly IEnumerable<SalesOrderRowJson> _rowsToAdd = [];
    private readonly IEnumerable<SalesOrderRowJson> _totalRows;
    
    private Guid _correlationId = Guid.CreateVersion7();

    public AddBeersToCartSuccessfully()
    {
        _rowsToAdd = _rowsToAdd.Concat(new List<SalesOrderRowJson>()
        {
            new SalesOrderRowJson
            {
                BeerId = Guid.CreateVersion7().ToString(),
                Quantity = new(2, "Bottles")
            }
        });
        
        _totalRows = _rowsToAdd.Union(_rows);
    }
    
    protected override IEnumerable<DomainEvent> Given()
    {
        yield return new SalesOrderCreated(_salesOrderId, _salesOrderNumber, _salesOrderDate, _customer,
            _salesOrderDeliveryDate, _rows, _correlationId);
    }

    protected override AddBeersToCart When() => new AddBeersToCart(_salesOrderId, _totalRows);

    protected override ICommandHandlerAsync<AddBeersToCart> OnHandler()
    {
        return new AddBeersToCartCommandHandler(Repository, new NullLoggerFactory());
    }

    protected override IEnumerable<DomainEvent> Expect()
    {
        yield return new BeersAddedToCart(_salesOrderId, _totalRows);
    }
}