using BrewUp.Shared.DomainIds;
using Muflone.Messages.Commands;

namespace BrewUp.Dashboards.SharedKernel.Messages.Commands;

public sealed class CreateSummaryForBeer(BeerId aggregateId): Command(aggregateId)
{
}