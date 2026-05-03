using BrewSpa.Dashboards.ApplicationServices.Models;
using BrewSpa.Dashboards.ApplicationServices.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BrewSpa.Dashboards.Facade;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject] private IDashboardService DashboardService { get; set; } = null!;

    private HubConnection? _hubConnection;

    private List<SalesByCustomerJson> _salesByCustomer = [];
    private List<SalesByProductJson> _salesByProduct = [];
    private bool _isLoading = true;
    private string _activeTab = "customers";

    private string _message = "Loading dashboard data...";

    protected override async Task OnInitializedAsync()
    {
      _hubConnection = new HubConnectionBuilder()
        .WithUrl(new Uri("http://localhost:5094/hubs/dashboards"))
        .WithAutomaticReconnect()
        .Build();
      
      _hubConnection.On<string>("DashboardsHubConnected", UpdateMessageDashboardAsync);
      _hubConnection.On<string>("CustomersDashboardUpdated", UpdateCustomersDashboardAsync);
      _hubConnection.On<string>("ProductsDashboardUpdated", UpdateProductsDashboardAsync);

      await _hubConnection.StartAsync();
      
      await GetSalesByCustomersAsync();
      await GetSalesByProductAsync();

      _isLoading = false;
    }

    private async Task GetSalesByCustomersAsync()
    {
        var customersResult = await DashboardService.GetSalesByCustomerAsync();
        customersResult.Match(
            success =>
            {
              _salesByCustomer = success.Results.ToList();
              return true;
            },
            error =>
            {
              Console.WriteLine($"[Dashboard] Error loading sales by customer: {error.Message}");
              return false;
            });
        
        StateHasChanged();
    }

    private async Task GetSalesByProductAsync()
    {
        var productsResult = await DashboardService.GetSalesByProductAsync();
        productsResult.Match(
            success =>
            {
                _salesByProduct = success.Results.ToList();
                return true;
            },
            error =>
            {
                Console.WriteLine($"[Dashboard] Error loading sales by product: {error.Message}");
                return false;
            });
        
        StateHasChanged();
    }

    private Task UpdateMessageDashboardAsync(string message)
    {
        _message = message;
        
        return Task.CompletedTask;
    }

    private async Task UpdateCustomersDashboardAsync(string customerId)
    {
        _message = $"Customer {customerId} updated. Refreshing dashboard data...";
        
        await GetSalesByCustomersAsync();
    }
    
    private Task UpdateProductsDashboardAsync(string productId) 
        => GetSalesByProductAsync();

    #region Dispose
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) 
            await _hubConnection.DisposeAsync();
    }
    #endregion
}
