using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BrewApp.Models.Sales;

namespace BrewApp.PageModels.Sales;

public partial class CreateSalesOrderPageModel : ObservableObject
{
    private readonly ISalesOrderService _salesOrderService;

    [ObservableProperty]
    private string orderNumber = string.Empty;

    [ObservableProperty]
    private DateTime orderDate = DateTime.Today;

    [ObservableProperty]
    private string customerId = string.Empty;

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private DateTime deliveryDate = DateTime.Today.AddDays(3);

    [ObservableProperty]
    private ObservableCollection<SalesOrderRow> rows = new();

    [ObservableProperty]
    private bool isSaving;

    public CreateSalesOrderPageModel(ISalesOrderService salesOrderService)
    {
        _salesOrderService = salesOrderService;
        
        // Initialize with one empty row
        Rows.Add(new SalesOrderRow
        {
            Quantity = new Quantity { UnitOfMeasure = "Bottles" },
            Price = new Price { Currency = "EUR" }
        });
    }

    [RelayCommand]
    public void AddRow()
    {
        Rows.Add(new SalesOrderRow
        {
            Quantity = new Quantity { UnitOfMeasure = "Bottles" },
            Price = new Price { Currency = "EUR" }
        });
    }

    [RelayCommand]
    public void RemoveRow(SalesOrderRow row)
    {
        if (Rows.Count > 1)
        {
            Rows.Remove(row);
        }
    }

    [RelayCommand]
    public async Task SaveOrderAsync()
    {
        if (IsSaving)
            return;

        if (!ValidateOrder())
            return;

        IsSaving = true;

        try
        {
            var request = new CreateSalesOrderRequest
            {
                OrderNumber = OrderNumber,
                OrderDate = OrderDate,
                CustomerId = CustomerId,
                CustomerName = CustomerName,
                DeliveryDate = DeliveryDate,
                Rows = Rows.ToList()
            };

            var result = await _salesOrderService.CreateSalesOrderAsync(request);

            if (result != null)
            {
                await Shell.Current.DisplayAlert("Success", "Sales order created successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to create sales order", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to create sales order: {ex.Message}", "OK");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    public async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private bool ValidateOrder()
    {
        if (string.IsNullOrWhiteSpace(OrderNumber))
        {
            Shell.Current.DisplayAlert("Validation Error", "Order number is required", "OK");
            return false;
        }

        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            Shell.Current.DisplayAlert("Validation Error", "Customer name is required", "OK");
            return false;
        }

        if (Rows.Count == 0)
        {
            Shell.Current.DisplayAlert("Validation Error", "At least one order row is required", "OK");
            return false;
        }

        return true;
    }
}
