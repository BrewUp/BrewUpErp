using BrewUp.Shared.DomainIds;
using Muflone.Messages.Commands;

namespace BrewUp.Dashboards.SharedKernel.Messages.Commands;

public sealed class CreateSummaryByBeer(BeerId aggregateId): Command(aggregateId)
{
}