namespace BrewUp.Chat.SharedKernel.Sales;

public sealed record SalesOrderSummary(
    string OrderId,
    string CustomerId,
    string CustomerName,
    string Status,
    DateOnly OrderDate,
    DateOnly? RequestedDeliveryDate,
    decimal TotalAmount);
