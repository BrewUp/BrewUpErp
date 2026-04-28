using BrewUp.Shared.DomainIds;
using Muflone.Messages.Commands;

namespace BrewUp.Dashboards.SharedKernel.Messages.Commands;

public class CreateSummaryByCustomer(CustomerId aggregateId) : Command(aggregateId)
{
}