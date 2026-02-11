namespace BrewApp.Mobile.Services.Api.Dtos;

/// <summary>
/// DTO for Order entity from the Orders API.
/// Matches the schema from contracts/mobile-app-apis.openapi.yaml.
/// </summary>
public class OrderDto
{
    public Guid? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<OrderLineItemDto>? LineItems { get; set; }
}

/// <summary>
/// DTO for OrderLineItem nested in Order.
/// </summary>
public class OrderLineItemDto
{
    public Guid? LineItemId { get; set; }
    public Guid? BeerId { get; set; }
    public string? BeerName { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Subtotal { get; set; }
}

/// <summary>
/// DTO for creating a new order (POST /orders).
/// </summary>
public class CreateOrderRequestDto
{
    public string? CustomerName { get; set; }
    public List<CreateOrderLineItemDto>? LineItems { get; set; }
}

/// <summary>
/// DTO for line items in a create order request.
/// </summary>
public class CreateOrderLineItemDto
{
    public Guid BeerId { get; set; }
    public int Quantity { get; set; }
}
