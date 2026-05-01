using BrewUp.Dashboards.SharedKernel.CustomTypes;
using BrewUp.Shared.DomainIds;
using Muflone.Messages.Commands;

namespace BrewUp.Dashboards.SharedKernel.Messages.Commands;

public class IncreaseSalesSummaryByCustomer(CustomerId aggregateId,
    SalesOrderValue salesOrderValue,
    SalesOrderYear salesOrderYear) : Command(aggregateId)
{
    public SalesOrderValue SalesOrderValue { get; private set; } = salesOrderValue;
    public SalesOrderYear SalesOrderYear { get; private set; } = salesOrderYear;
}