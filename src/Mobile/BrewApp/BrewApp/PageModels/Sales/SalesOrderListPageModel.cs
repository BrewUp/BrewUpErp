using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BrewApp.Models.Sales;
using BrewApp.Services.Sales;

namespace BrewApp.PageModels.Sales;

public partial class SalesOrderListPageModel : ObservableObject
{
    private readonly ISalesOrderService _salesOrderService;

    [ObservableProperty]
    private ObservableCollection<SalesOrder> salesOrders = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    public SalesOrderListPageModel(ISalesOrderService salesOrderService)
    {
        _salesOrderService = salesOrderService;
    }

    [RelayCommand]
    public async Task LoadSalesOrdersAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            var response = await _salesOrderService.GetSalesOrdersAsync();
            SalesOrders.Clear();
            
            foreach (var order in response.Results)
            {
                SalesOrders.Add(order);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load sales orders: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadSalesOrdersAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    public async Task NavigateToCreateOrderAsync()
    {
        await Shell.Current.GoToAsync("createorder");
    }

    [RelayCommand]
    public async Task NavigateToOrderDetailAsync(string orderId)
    {
        if (!string.IsNullOrEmpty(orderId))
        {
            await Shell.Current.GoToAsync($"orderdetail?id={orderId}");
        }
    }
}
