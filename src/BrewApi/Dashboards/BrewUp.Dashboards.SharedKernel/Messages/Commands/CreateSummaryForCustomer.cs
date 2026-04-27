using BrewUp.Shared.DomainIds;
using Muflone.Messages.Commands;

namespace BrewUp.Dashboards.SharedKernel.Messages.Commands;

public class CreateSummaryForCustomer(CustomerId aggregateId) : Command(aggregateId)
{
}