using BrewUp.Sales.Domain.CommandHandlers;
using BrewUp.Sales.SharedKernel.CustomTypes;
using BrewUp.Sales.SharedKernel.Messages.Commands;
using BrewUp.Sales.SharedKernel.Messages.Events;
using BrewUp.Shared.ExternalContracts;
using Microsoft.Extensions.Logging.Abstractions;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;
using Muflone.SpecificationTests;

namespace BrewUp.Sales.Tests.Domain;

public sealed class CreateSalesOrderSuccessfully : CommandSpecification<CreateSalesOrder>
{
    private SalesOrderId _salesOrderId = new (Guid.CreateVersion7().ToString());
    private SalesOrderNumber _salesOrderNumber = new ("SO-1000");
    private CustomerId _customerId = new (Guid.CreateVersion7().ToString());
    private CustomerName _customerName = new ("John Doe");
    private SalesOrderDate _salesOrderDate = new (DateTime.UtcNow);
    private SalesOrderDeliveryDate _salesOrderDeliveryDate = new (DateTime.UtcNow.AddDays(7));
    private List<SalesOrderRowJson> _rows = [];
    
    private Guid _correlationId = Guid.CreateVersion7();
    
    protected override IEnumerable<DomainEvent> Given()
    {
        yield break;
    }

    protected override CreateSalesOrder When() => new (_salesOrderId, _salesOrderNumber, _salesOrderDate, 
        _customerId, _customerName, _salesOrderDeliveryDate, _rows, _correlationId);

    protected override ICommandHandlerAsync<CreateSalesOrder> OnHandler()
        => new CreateSalesOrderCommandHandler(Repository, new NullLoggerFactory());
    

    protected override IEnumerable<DomainEvent> Expect()
    {
        yield return new SalesOrderCreated(_salesOrderId, _salesOrderNumber, _salesOrderDate, _customerId,
            _customerName, _salesOrderDeliveryDate, _rows, _correlationId);
    }
}