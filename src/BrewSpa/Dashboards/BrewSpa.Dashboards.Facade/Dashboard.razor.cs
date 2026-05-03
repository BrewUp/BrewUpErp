using BrewSpa.Dashboards.ApplicationServices.Models;
using BrewSpa.Dashboards.ApplicationServices.Services;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.Dashboards.Facade;

public partial class Dashboard : ComponentBase, IDisposable
{
    [Inject] private IDashboardService DashboardService { get; set; } = null!;

    private List<SalesByCustomerJson> _salesByCustomer = [];
    private List<SalesByProductJson> _salesByProduct = [];
    private bool _isLoading = true;
    private string _activeTab = "customers";

    protected override async Task OnInitializedAsync()
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

      _isLoading = false;
    }

    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Dashboard()
    {
        Dispose(false);
    }
    #endregion
}
