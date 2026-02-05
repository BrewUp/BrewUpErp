using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BrewSpa.MasterData.Application.Models;
using BrewSpa.MasterData.Application.Services;

namespace BrewSpa.MasterData.Facade.Components;

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

        result.Match(
            success =>
            {
                _customers = success.Results;
                _totalRecords = success.TotalRecords;
                return true;
            },
            error =>
            {
                _ = ShowError($"Error loading customers: {error.Message}");
                _customers = [];
                _totalRecords = 0;
                return false;
            });
        
        _isLoading = false;
        StateHasChanged();
    }

    private void SelectCustomer(CustomerJson customer)
    {
        if (_editingCustomer != null)
        {
            // Don't allow selection change while editing
            return;
        }
        _selectedCustomer = customer;
        StateHasChanged();
    }

    private void AddCustomer()
    {
        // var newCustomer = new CustomerJson
        // {
        //     CustomerId = Guid.NewGuid().ToString(),
        //     Indirizzo = new Address
        //     {
        //         Nazione = "ITA"
        //     }
        // };
        //
        // _customers.Insert(0, newCustomer);
        // EditCustomer(newCustomer);
        // StateHasChanged();
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

    private string GetConsumerLevelClass(string level)
    {
        return level?.ToLower() switch
        {
            "teetotaler" => "secondary",
            "moderate" => "info",
            "regular" => "primary",
            "enthusiast" => "success",
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
