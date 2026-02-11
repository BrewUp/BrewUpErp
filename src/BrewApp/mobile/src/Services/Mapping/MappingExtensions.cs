using BrewApp.Mobile.Models;
using BrewApp.Mobile.Services.Api.Dtos;

namespace BrewApp.Mobile.Services.Mapping;

/// <summary>
/// Extension methods for mapping between API DTOs and internal view models.
/// Follows the entity definitions from data-model.md.
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps API DTO Beer to internal Beer model.
    /// </summary>
    public static Beer ToBeer(this BeerDto dto)
    {
        return new Beer
        {
            BeerId = dto.BeerId ?? Guid.Empty,
            Name = dto.Name ?? string.Empty,
            Style = dto.Style ?? string.Empty,
            Abv = dto.Abv ?? 0.0m,
            Description = dto.Description ?? string.Empty,
            IsAvailable = dto.IsAvailable ?? true
        };
    }

    /// <summary>
    /// Maps API DTO Order to internal Order model.
    /// </summary>
    public static Order ToOrder(this OrderDto dto)
    {
        return new Order
        {
            OrderId = dto.OrderId ?? Guid.Empty,
            CustomerName = dto.CustomerName ?? string.Empty,
            OrderDate = dto.OrderDate ?? DateTime.MinValue,
            Status = dto.Status ?? "Unknown",
            TotalAmount = dto.TotalAmount ?? 0.0m,
            LineItems = dto.LineItems?.Select(li => li.ToOrderLineItem()).ToList() ?? new List<OrderLineItem>()
        };
    }

    /// <summary>
    /// Maps API DTO OrderLineItem to internal OrderLineItem model.
    /// </summary>
    public static OrderLineItem ToOrderLineItem(this OrderLineItemDto dto)
    {
        return new OrderLineItem
        {
            LineItemId = dto.LineItemId ?? Guid.Empty,
            BeerId = dto.BeerId ?? Guid.Empty,
            BeerName = dto.BeerName ?? string.Empty,
            Quantity = dto.Quantity ?? 0,
            UnitPrice = dto.UnitPrice ?? 0.0m,
            Subtotal = dto.Subtotal ?? 0.0m
        };
    }

    /// <summary>
    /// Maps API DTO SalesSummary to internal SalesSummary model.
    /// </summary>
    public static SalesSummary ToSalesSummary(this SalesSummaryDto dto)
    {
        return new SalesSummary
        {
            Period = dto.Period ?? "Unknown",
            TotalRevenue = dto.TotalRevenue ?? 0.0m,
            OrdersCount = dto.OrdersCount ?? 0,
            AverageOrderValue = dto.AverageOrderValue ?? 0.0m,
            TopSellingBeers = dto.TopSellingBeers ?? new List<string>()
        };
    }

    /// <summary>
    /// Maps API DTO StockItem to internal StockItem model.
    /// </summary>
    public static StockItem ToStockItem(this StockItemDto dto)
    {
        return new StockItem
        {
            BeerId = dto.BeerId ?? Guid.Empty,
            BeerName = dto.BeerName ?? string.Empty,
            Location = dto.Location ?? string.Empty,
            QuantityOnHand = dto.QuantityOnHand ?? 0,
            Unit = dto.Unit ?? "units"
        };
    }

    /// <summary>
    /// Maps internal Order to API CreateOrderRequest DTO.
    /// </summary>
    public static CreateOrderRequestDto ToCreateOrderRequest(this Order order)
    {
        return new CreateOrderRequestDto
        {
            CustomerName = order.CustomerName,
            LineItems = order.LineItems.Select(li => new CreateOrderLineItemDto
            {
                BeerId = li.BeerId,
                Quantity = li.Quantity
            }).ToList()
        };
    }
}
