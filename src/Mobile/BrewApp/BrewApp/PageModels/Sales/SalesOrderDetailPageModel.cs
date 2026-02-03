using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BrewApp.Models.Sales;
using BrewApp.Services.Sales;

namespace BrewApp.PageModels.Sales;

[QueryProperty(nameof(OrderId), "id")]
public partial class SalesOrderDetailPageModel : ObservableObject
{
    private readonly ISalesOrderService _salesOrderService;

    [ObservableProperty]
    private string orderId = string.Empty;

    [ObservableProperty]
    private SalesOrder? salesOrder;

    [ObservableProperty]
    private bool isLoading;

    public SalesOrderDetailPageModel(ISalesOrderService salesOrderService)
    {
        _salesOrderService = salesOrderService;
    }

    partial void OnOrderIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadOrderDetailsAsync();
        }
    }

    [RelayCommand]
    public async Task LoadOrderDetailsAsync()
    {
        if (string.IsNullOrEmpty(OrderId) || IsLoading)
            return;

        IsLoading = true;

        try
        {
            SalesOrder = await _salesOrderService.GetSalesOrderByIdAsync(OrderId);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load order details: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
