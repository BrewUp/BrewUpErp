using BrewSpa.Sales.Application.Models;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.Sales.Facade.Components.Orders;

public partial class AddSalesOrder : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public SalesOrderJson? Order { get; set; }
    [Parameter] public EventCallback<SalesOrderJson> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private void AddOrderRow()
    {
        if (Order == null) 
            return;

        var item = new SalesOrderRowJson
        {
            BeerId = "",
            BeerName = "",
            Quantity = new Quantity { Value = 1, UnitOfMeasure = "L" },
            Price = new Price { Value = 0, Currency = "EUR" }
        };
        Order.Rows.Add(item);
        StateHasChanged();
    }

    private void RemoveOrderRow(int index)
    {
        if (Order == null || index < 0 || index >= Order.Rows.Count) 
            return;
        
        Order.Rows.RemoveAt(index);
        StateHasChanged();
    }

    private bool IsValid()
    {
        if (Order == null) return false;
        
        return !string.IsNullOrWhiteSpace(Order.OrderNumber) &&
               !string.IsNullOrWhiteSpace(Order.CustomerId) &&
               !string.IsNullOrWhiteSpace(Order.CustomerName) &&
               Order.Rows.Any() &&
               Order.Rows.All(r => 
                   !string.IsNullOrWhiteSpace(r.BeerId) &&
                   !string.IsNullOrWhiteSpace(r.BeerName) &&
                   r.Quantity.Value > 0 &&
                   !string.IsNullOrWhiteSpace(r.Quantity.UnitOfMeasure) &&
                   r.Price.Value > 0 &&
                   !string.IsNullOrWhiteSpace(r.Price.Currency));
    }

    private async Task HandleSubmit()
    {
        if (IsValid() && Order != null)
        {
            await OnSubmit.InvokeAsync(Order);
        }
    }
}
