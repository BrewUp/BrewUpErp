using Blazor.Messaging;
using BrewSpa.Sales.Application.Models;
using BrewSpa.Sales.Application.Services;
using BrewSpa.Shared.Components.CustomTypes;
using BrewSpa.Shared.Components.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;

namespace BrewSpa.Sales.Facade.Components.Orders;

public partial class SalesOrders : ComponentBase, IDisposable
{
    [Inject] private ISalesService SalesService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IMessagingService MessagingService { get; set; } = null!;

    private IEnumerable<SalesOrderJson> _salesOrders = [];
    private GridItemsProvider<SalesOrderJson>? _gridItemsProvider;
    private readonly PaginationState _pagination = new() { ItemsPerPage = 10 };
    private string _orderNumberFilter = "";
    private SalesOrderJson? _selectedOrder;
    private bool _isLoading = true;
    private bool _showDialog;

    private readonly CurrentContext _currentContext = new ("SalesOrders");

    private int _currentPage = 1;
    private readonly int _pageSize = 10;
    private int _totalRecords;

    protected override async Task OnInitializedAsync()
    {
        MessagingService.Subscribe<ToolbarItemClicked>(HandleToolbarClickAsync);
        await LoadSalesOrders();
    }

    private async Task LoadSalesOrders()
    {
        _isLoading = true;
        StateHasChanged();

        var result = await SalesService.GetSalesOrdersAsync(_currentPage, _pageSize);

        result.Match(
            success =>
            {
                _salesOrders = success.Results;
                _totalRecords = success.TotalRecords;
                _isLoading = false;

                _gridItemsProvider = _ => ValueTask.FromResult(GridItemsProviderResult.From(
                    items: _salesOrders.ToArray(),
                    totalItemCount: _totalRecords
                ));

                return true;
            },
            error =>
            {
                _ = ShowError($"Error loading sales orders: {error.Message}");
                _salesOrders = [];
                _totalRecords = 0;
                _isLoading = false;

                _gridItemsProvider = _ => ValueTask.FromResult(GridItemsProviderResult.From(
                    items: Array.Empty<SalesOrderJson>(),
                    totalItemCount: 0
                ));

                return false;
            });

        StateHasChanged();
    }

    private async Task HandleToolbarClickAsync(ToolbarItemClicked message)
    {
        if (message.CurrentContext != _currentContext) return;

        switch (message.ToolbarButton.Name)
        {
            case nameof(ToolbarButtons.AddNewItem):
                AddNewOrder();
                break;

            case nameof(ToolbarButtons.Refresh):
                await RefreshData();
                break;

            case nameof(ToolbarButtons.Close):
                Close();
                break;
        }
    }

    private void SelectOrder(SalesOrderJson order)
    {
        _selectedOrder = order;
        Console.WriteLine($"Selected order: {_selectedOrder?.OrderNumber ?? "null"}");
        StateHasChanged();
    }

    private string GetRowClass(SalesOrderJson order)
    {
        var baseClass = "grid-row";
        if (_selectedOrder != null && _selectedOrder.Id == order.Id)
        {
            return $"{baseClass} selected-row";
        }
        return baseClass;
    }

    private void AddNewOrder()
    {
        _selectedOrder = new SalesOrderJson
        {
            OrderNumber = "",
            OrderDate = DateTime.Now,
            CustomerName = "",
            CustomerId = "",
            DeliveryDate = DateTime.Now.AddDays(7),
            Rows = []
        };

        _showDialog = true;
        StateHasChanged();
    }

    private void ViewOrderDetails(SalesOrderJson order)
    {
        Console.WriteLine($"View details for order: {order.OrderNumber}");
        // TODO: Navigate to order details page or open a modal
    }

    private async Task OnDialogSubmit(SalesOrderJson order)
    {
        _showDialog = false;

        var createOrder = new CreateSalesOrderJson
        {
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            DeliveryDate = order.DeliveryDate,
            Rows = order.Rows
        };

        var result = await SalesService.CreateSalesOrderViaSagaAsync(createOrder);

        result.Match(
            success =>
            {
                _ = ShowSuccess("Sales order created successfully");
                _ = LoadSalesOrders();
                return true;
            },
            error =>
            {
                _ = ShowError($"Failed to create sales order: {error.Message}");
                return false;
            });
    }

    private void OnDialogCancel()
    {
        _showDialog = false;
        _selectedOrder = null;
        StateHasChanged();
    }

    private async Task RefreshData()
    {
        Console.WriteLine("RefreshData button clicked");
        await LoadSalesOrders();
    }

    private void Close()
    {
        Navigation.NavigateTo("/sales");
    }

    private async Task ShowError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
        await JsRuntime.InvokeVoidAsync("alert", message);
    }

    private async Task ShowSuccess(string message)
    {
        Console.WriteLine($"[SUCCESS] {message}");
        await JsRuntime.InvokeVoidAsync("alert", message);
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

    ~SalesOrders()
    {
        Dispose(false);
    }
    #endregion   
}