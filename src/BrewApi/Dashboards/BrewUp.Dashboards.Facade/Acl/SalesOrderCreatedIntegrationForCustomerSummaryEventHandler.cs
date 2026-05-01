using System.Linq.Expressions;
using BrewUp.Dashboards.Entities.Dtos;
using BrewUp.Dashboards.Infrastructure;
using BrewUp.Dashboards.SharedKernel.CustomTypes;
using BrewUp.Dashboards.SharedKernel.Messages.Commands;
using BrewUp.Shared.DomainIds;
using BrewUp.Shared.Messages.Events;
using BrewUp.Shared.ReadModel;
using Microsoft.Extensions.Logging;
using Muflone.Messages.Commands;
using Muflone.Messages.Events;

namespace BrewUp.Dashboards.Facade.Acl;

public sealed class SalesOrderCreatedIntegrationForCustomerSummaryEventHandler(
    ICommandHandlerAsync<IncreaseSalesSummaryByCustomer> increaseSalesSummaryByCustomerCommandHandler, 
    ICommandHandlerAsync<CreateSummaryByCustomer> createSalesSummaryByCustomerCommandHandler,
    IMessagesReceivedService  messagesReceivedService,
    IQueries<SalesByCustomers> salesByCustomersQueries,
    ILoggerFactory loggerFactory)
    : IntegrationEventHandlerAsync<SalesOrderCreatedWihPriceIntegrationEvent>(loggerFactory)
{
    public override async Task HandleAsync(SalesOrderCreatedWihPriceIntegrationEvent @event,
        CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (IsMessageAlreadyProcessed(@event.MessageId.ToString()))
            return;

        var salesOrderValue = @event.Rows.Sum(row => (double) (row.Price.Value * row.Quantity.Value));
        
        Expression<Func<SalesByCustomers, bool>> query = customers => customers.Id == @event.CustomerId && customers.Year == @event.SalesOrderDate.Year.ToString(); 
        var queryResult = await salesByCustomersQueries.GetByFilterAsync(query, 1, 1, cancellationToken);
        
        if (queryResult.IsError)
            return;
        
        queryResult.TryGetValue(out var pagedResult);
            
        if (!pagedResult.Results.Any())
        {
            CreateSummaryByCustomer createCommand = new (new CustomerId(@event.CustomerId),
                new CustomerName(@event.CustomerName),
                new SalesOrderValue(salesOrderValue, "EUR"),
                new SalesOrderYear(@event.SalesOrderDate.Year.ToString()));
        
            await createSalesSummaryByCustomerCommandHandler.HandleAsync(createCommand, cancellationToken); 
        }
            
        salesOrderValue += pagedResult.Results.FirstOrDefault()?.TotalSales ?? 0;
            
        IncreaseSalesSummaryByCustomer increaseCommand = new (new CustomerId(@event.CustomerId),
            new SalesOrderValue(salesOrderValue, "EUR"),
            new SalesOrderYear(@event.SalesOrderDate.Year.ToString()));
            
        await increaseSalesSummaryByCustomerCommandHandler.HandleAsync(increaseCommand, cancellationToken);
        
        await AddMessageAsync(@event.MessageId.ToString(), cancellationToken);
    }
    
    private bool IsMessageAlreadyProcessed(string messageId)
    {
        var result = messagesReceivedService.GetByIdAsync(messageId, CancellationToken.None).GetAwaiter().GetResult();
        return result.IsSuccess;
    }
    
    private Task AddMessageAsync(string messageId, CancellationToken cancellationToken)
    {
        var message = MessagesReceived.Create(Guid.Parse(messageId), nameof(SalesOrderCreatedWihPriceIntegrationEvent));
        return messagesReceivedService.AddAsync(message, cancellationToken);
    }
}