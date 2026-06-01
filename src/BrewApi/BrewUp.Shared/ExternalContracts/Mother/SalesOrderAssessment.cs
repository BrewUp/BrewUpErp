using BrewUp.Shared.ExternalContracts.Sales;

namespace BrewUp.Shared.ExternalContracts.Mother;

public sealed record SalesOrderAssessment(
    string SalesOrderId,
    string CustomerId,
    string CustomerName,
    decimal TotalAmount,
    IReadOnlyCollection<SalesOrderRowJson> Rows,
    string Priority,
    string Reason);