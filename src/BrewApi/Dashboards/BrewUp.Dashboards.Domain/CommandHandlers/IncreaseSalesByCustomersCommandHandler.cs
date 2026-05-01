using BrewUp.Dashboards.Entities.Dtos;
using BrewUp.Dashboards.SharedKernel.Messages.Commands;
using BrewUp.Dashboards.SharedKernel.Persistence;
using Microsoft.Extensions.Logging;

namespace BrewUp.Dashboards.Domain.CommandHandlers;

public sealed class IncreaseSalesByCustomersCommandHandler(IDashboardsRepository<SalesByCustomers> repository, ILoggerFactory loggerFactory) 
    : DashboardsCommandHandlerBaseAsync<IncreaseSalesSummaryByCustomer>(repository, loggerFactory)
{
    public override async Task HandleAsync(IncreaseSalesSummaryByCustomer command, CancellationToken cancellationToken = new ())
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        
    }
}