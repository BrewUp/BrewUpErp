using BrewSpa.MasterData.Application.Models;
using BrewSpa.MasterData.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BrewSpa.MasterData.Facade.Components.Customers;

public partial class Customers : ComponentBase, IDisposable
{
    [Inject] private ICustomerService CustomerService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private IEnumerable<CustomerJson> _customers = [];
    private CustomerJson? _selectedCustomer;
    private CustomerJson? _editingCustomer;
    private bool _isLoading = true;
    private bool HasChanges => _editingCustomer != null;
    
    private bool _showDialog = false;
    private CreateCustomerJson _dialogCustomer = new();
    
    private int _currentPage;
    private int _pageSize = 10;
    private int _totalRecords;
    private int TotalPages => (int)Math.Ceiling((double)_totalRecords / _pageSize);

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomers();
    }

    private async Task LoadCustomers()
    {
        _isLoading = true;
        StateHasChanged();
            
        var result = await CustomerService.GetCustomersAsync(_currentPage, _pageSize);

        _isLoading = result.Match(
            success =>
            {
                _customers = success.Results;
                _totalRecords = success.TotalRecords;
                return false;
            },error =>
            {
                _ = ShowError($"Error loading customers: {error.Message}");
                _customers = [];
                _totalRecords = 0;
                return true;
            });
        
        StateHasChanged();
    }

    private void SelectCustomer(CustomerJson customer)
    {
        Console.WriteLine($"SelectCustomer called with customer: {customer?.RagioneSociale ?? "null"}");
        if (_editingCustomer != null)
        {
            Console.WriteLine("Cannot select customer while editing");
            // Don't allow selection change while editing
            return;
        }
        _selectedCustomer = customer;
        Console.WriteLine($"Selected customer: {_selectedCustomer?.RagioneSociale ?? "null"}");
        StateHasChanged();
    }

    private void AddCustomer()
    {
        _dialogCustomer = new CreateCustomerJson
        {
            RagioneSociale = "",
            PartitaIva = "",
            Indirizzo = new IndirizzoJson
            {
                Via = "",
                NumeroCivico = "",
                Citta = "",
                Provincia = "",
                Cap = "",
                Nazione = "ITA"
            }
        };
    
        _showDialog = true;
        StateHasChanged();
    }

    private async Task OnDialogSubmit(CreateCustomerJson customer)
    {
        _showDialog = false;
        StateHasChanged();
    }
    
    private void OnDialogCancel()
    {
        _showDialog = false;
        StateHasChanged();
    }

    private void EditCustomer(CustomerJson customer)
    {
        // _editingCustomer = new Customer
        // {
        //     CustomerId = customer.CustomerId,
        //     RagioneSociale = customer.RagioneSociale,
        //     PartitaIva = customer.PartitaIva,
        //     ConsumerLevel = customer.ConsumerLevel,
        //     Indirizzo = new Address
        //     {
        //         Via = customer.Indirizzo.Via,
        //         NumeroCivico = customer.Indirizzo.NumeroCivico,
        //         Citta = customer.Indirizzo.Citta,
        //         Provincia = customer.Indirizzo.Provincia,
        //         Cap = customer.Indirizzo.Cap,
        //         Nazione = customer.Indirizzo.Nazione
        //     }
        // };
        StateHasChanged();
    }

    private async Task SaveEdit()
    {
        if (_editingCustomer == null) return;

        if (string.IsNullOrWhiteSpace(_editingCustomer.RagioneSociale))
        {
            await ShowError("Company name is required.");
            return;
        }

        try
        {
            CustomerJson savedCustomer;
            var existingCustomer = _customers.FirstOrDefault(c => c.CustomerId == _editingCustomer.CustomerId);
            
            if (existingCustomer != null && !string.IsNullOrEmpty(existingCustomer.RagioneSociale))
            {
                // Update existing customer
                var updateResult = await CustomerService.UpdateCustomerAsync(_editingCustomer);
            }
            else
            {
                // Create new customer
                var saveResult = await CustomerService.CreateCustomerAsync(_editingCustomer);
            }

            // if (existingCustomer != null)
            // {
            //     var index = _customers.IndexOf(existingCustomer);
            //     _customers[index] = savedCustomer;
            // }
            
            _editingCustomer = null;
            await ShowSuccess("Customer saved successfully!");
        }
        catch (Exception ex)
        {
            await ShowError($"Error saving customer: {ex.Message}");
        }
        
        StateHasChanged();
    }

    private void CancelEdit()
    {
        if (_editingCustomer != null)
        {
            // If it's a new customer (not saved yet), remove it from the list
            var existingInOriginal = _customers.FirstOrDefault(c => c.CustomerId == _editingCustomer.CustomerId);
            if (existingInOriginal != null && string.IsNullOrEmpty(existingInOriginal.RagioneSociale))
                _customers = _customers.Where(c => c.CustomerId != existingInOriginal.CustomerId);
        }
        
        _editingCustomer = null;
        StateHasChanged();
    }

    private async Task SaveCustomers()
    {
        Console.WriteLine("SaveCustomers button clicked");
        await JsRuntime.InvokeVoidAsync("alert", "Save button clicked!");
        try
        {
            // TODO: Implement bulk save logic here
            await ShowSuccess("All changes saved!");
        }
        catch (Exception ex)
        {
            await ShowError($"Error saving changes: {ex.Message}");
        }
    }

    private async Task DeleteCustomer()
    {
        if (_selectedCustomer == null) return;

        var confirmed = await JsRuntime.InvokeAsync<bool>("confirm", 
            $"Are you sure you want to delete customer '{_selectedCustomer.RagioneSociale}'?");
        
        if (confirmed)
        {
            try
            {
                var deleted = await CustomerService.DeleteCustomerAsync(_selectedCustomer.CustomerId);
                
                if (deleted.IsSuccess)
                {
                    _customers = _customers.Where(c => c.CustomerId != _selectedCustomer.CustomerId);
                    _selectedCustomer = null;
                    _totalRecords = Math.Max(0, _totalRecords - 1);
                    
                    await ShowSuccess("Customer deleted successfully!");
                    StateHasChanged();
                }
                else
                {
                    await ShowError("Failed to delete customer.");
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error deleting customer: {ex.Message}");
            }
        }
    }

    private async Task RefreshData()
    {
        Console.WriteLine("RefreshData button clicked");
        _editingCustomer = null;
        _selectedCustomer = null;
        _currentPage = 0;
        await LoadCustomers();
    }

    private async Task PreviousPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            await LoadCustomers();
        }
    }

    private async Task NextPage()
    {
        if (_currentPage < TotalPages - 1)
        {
            _currentPage++;
            await LoadCustomers();
        }
    }

    private async Task OnPageSizeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var newPageSize))
        {
            _pageSize = newPageSize;
            _currentPage = 0;
            await LoadCustomers();
        }
    }

    private void Close()
    {
        Navigation.NavigateTo("/masterdata");
    }

    private static string GetConsumerLevelClass(string level)
    {
        return level?.ToLower() switch
        {
            "teetotaler" => "teetotaler",
            "gold" => "teetotaler",
            "silver" => "silver",
            "bronze " => "bronze",
            _ => "light"
        };
    }

    private async Task ShowSuccess(string message)
    {
        await JsRuntime.InvokeVoidAsync("alert", message);
    }

    private async Task ShowError(string message)
    {
        await JsRuntime.InvokeVoidAsync("alert", message);
    }

    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup resources if needed
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Customers()
    {
        Dispose(false);
    }
    #endregion
}
