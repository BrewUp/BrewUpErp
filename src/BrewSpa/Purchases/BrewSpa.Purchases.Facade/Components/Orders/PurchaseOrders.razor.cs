using Blazor.Messaging;
using BrewSpa.Purchases.Application.Models;
using BrewSpa.Purchases.Application.Services;
using BrewSpa.Shared.Components.CustomTypes;
using BrewSpa.Shared.Components.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BrewSpa.Purchases.Facade.Components.Orders;

public partial class PurchaseOrders : ComponentBase, IDisposable
{
    [Inject] private IPurchaseService PurchaseService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private IMessagingService MessagingService { get; set; } = null!;

    private bool _isLoading;
    private bool _showDialog;
    private CreatePurchaseOrderJson? _currentOrder;
    private string? _successMessage;
    private string? _errorMessage;

    private readonly CurrentContext _currentContext = new("PurchaseOrders");

    protected override void OnInitialized()
    {
        MessagingService.Subscribe<ToolbarItemClicked>(HandleToolbarClickAsync);
    }

    private Task HandleToolbarClickAsync(ToolbarItemClicked message)
    {
        if (message.CurrentContext != _currentContext) return Task.CompletedTask;

        switch (message.ToolbarButton.Name)
        {
            case nameof(ToolbarButtons.AddNewItem):
                OpenNewOrderDialog();
                break;

            case nameof(ToolbarButtons.Refresh):
                DismissSuccess();
                DismissError();
                StateHasChanged();
                break;

            case nameof(ToolbarButtons.Close):
                break;
        }

        return Task.CompletedTask;
    }

    private void OpenNewOrderDialog()
    {
        var orderDate = DateTime.UtcNow;
        _currentOrder = new CreatePurchaseOrderJson
        {
            OrderNumber = $"PO-{orderDate:yyyyMMdd-HHmm}",
            OrderDate = orderDate,
            DeliveryDate = orderDate.AddDays(14),
            Rows = [new PurchaseOrderRowJson
            {
                Quantity = new Quantity { Value = 1, UnitOfMeasure = "pcs" },
                Price = new Price { Value = 0, Currency = "EUR" }
            }]
        };
        _showDialog = true;
        StateHasChanged();
    }

    private async Task OnDialogSubmit(CreatePurchaseOrderJson order)
    {
        _showDialog = false;
        _isLoading = true;
        _errorMessage = null;
        _successMessage = null;
        StateHasChanged();

        var result = await PurchaseService.CreatePurchaseOrderAsync(order);

        result.Match(
            _ =>
            {
                _successMessage = $"Purchase order '{order.OrderNumber}' created successfully.";
                _currentOrder = null;
                _isLoading = false;
                return true;
            },
            error =>
            {
                _errorMessage = "Failed to create purchase order. Please try again.";
                _isLoading = false;
                return false;
            });

        StateHasChanged();
    }

    private void OnDialogCancel()
    {
        _showDialog = false;
        _currentOrder = null;
        StateHasChanged();
    }

    private void DismissSuccess()
    {
        _successMessage = null;
        StateHasChanged();
    }

    private void DismissError()
    {
        _errorMessage = null;
        StateHasChanged();
    }

    private async Task ShowError(string message)
    {
        await JsRuntime.InvokeVoidAsync("console.error", message);
    }

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            MessagingService.Unsubscribe<ToolbarItemClicked>(HandleToolbarClickAsync);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~PurchaseOrders()
    {
        Dispose(false);
    }

    #endregion
}
