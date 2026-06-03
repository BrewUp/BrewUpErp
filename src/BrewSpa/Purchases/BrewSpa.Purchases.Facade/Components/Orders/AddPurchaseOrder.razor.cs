using BrewSpa.Purchases.Application.Models;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.Purchases.Facade.Components.Orders;

public partial class AddPurchaseOrder : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public CreatePurchaseOrderJson? Order { get; set; }
    [Parameter] public EventCallback<CreatePurchaseOrderJson> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private bool ShowValidation { get; set; }

    private void AddRow()
    {
        if (Order == null) return;

        Order.Rows.Add(new PurchaseOrderRowJson
        {
            Quantity = new Quantity { Value = 1, UnitOfMeasure = "pcs" },
            Price = new Price { Value = 0, Currency = "EUR" }
        });
        StateHasChanged();
    }

    private void RemoveRow(int index)
    {
        if (Order == null || index < 0 || index >= Order.Rows.Count) return;
        Order.Rows.RemoveAt(index);
        StateHasChanged();
    }

    private bool IsValid()
    {
        if (Order == null) return false;

        return !string.IsNullOrWhiteSpace(Order.OrderNumber) &&
               !string.IsNullOrWhiteSpace(Order.SupplierId) &&
               Order.Rows.Count > 0 &&
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
        ShowValidation = true;
        StateHasChanged();

        if (IsValid() && Order != null)
        {
            ShowValidation = false;
            await OnSubmit.InvokeAsync(Order);
        }
    }
}
