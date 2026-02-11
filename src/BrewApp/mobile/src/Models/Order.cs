namespace BrewApp.Mobile.Models;

/// <summary>
/// Internal model for Order entity.
/// Represents a sales order placed by a customer.
/// </summary>
public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// Internal model for OrderLineItem.
/// Represents a line item (beer + quantity) within an order.
/// </summary>
public class OrderLineItem
{
    public Guid LineItemId { get; set; }
    public Guid BeerId { get; set; }
    public string BeerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
