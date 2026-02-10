using Blazor.Messaging;
using BrewSpa.MasterData.Application.Models;
using BrewSpa.MasterData.Application.Services;
using BrewSpa.Shared.Components.CustomTypes;
using BrewSpa.Shared.Components.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;

namespace BrewSpa.MasterData.Facade.Components.Customers;

public partial class Customers : ComponentBase, IDisposable
{
    [Inject] private ICustomerService CustomerService { get; set; } = null!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IMessagingService MessagingService { get; set; } = null!;

    private IEnumerable<CustomerJson> _customers = [];
    private GridItemsProvider<CustomerJson>? _gridItemsProvider;
    private readonly PaginationState _pagination = new()  { ItemsPerPage = 10 };
    private string _nameFilter = "";
    private CustomerJson? _selectedCustomer = new();
    private CustomerJson? _editingCustomer;
    private bool _isLoading = true;
    
    private bool _showDialog;
    
    private readonly CurrentContext _currentContext = new ("Customers");
    
    private int _currentPage;
    private readonly int _pageSize = 10;
    private int _totalRecords;

    protected override async Task OnInitializedAsync()
    {
        MessagingService.Subscribe<ToolbarItemClicked>(HandleToolbarClickAsync);
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
                _isLoading = false;
                
                // Create the GridItemsProvider from the customers data
                _gridItemsProvider = _ => ValueTask.FromResult(GridItemsProviderResult.From(
                    items: _customers.ToArray(),
                    totalItemCount: _totalRecords
                ));
                
                return true;
            },
            error =>
            {
                _ = ShowError($"Error loading customers: {error.Message}");
                _customers = [];
                _totalRecords = 0;
                _isLoading = false;
                
                // Create an empty GridItemsProvider for error case
                _gridItemsProvider = _ => ValueTask.FromResult(GridItemsProviderResult.From(
                    items: Array.Empty<CustomerJson>(),
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
                AddCustomer();
                break;
            
            case nameof(ToolbarButtons.EditCurrentItem):
                EditCustomer();
                break;
            
            case nameof(ToolbarButtons.DeleteCurrentItem):
                await DeleteCustomer();
                break;
            
            case nameof(ToolbarButtons.Refresh):
                await RefreshData();
                break;
            
            case nameof(ToolbarButtons.Close):
                Close();
                break;
        }
    }

    private void SelectCustomer(CustomerJson customer)
    {
        if (_editingCustomer != null)
        {
            // Don't allow selection change while editing
            return;
        }
        _selectedCustomer = customer;
        Console.WriteLine($"Selected customer: {_selectedCustomer?.RagioneSociale ?? "null"}");
        StateHasChanged();
    }

    private string GetRowClass(CustomerJson customer)
    {
        var baseClass = "grid-row";
        if (_selectedCustomer != null && _selectedCustomer.CustomerId == customer.CustomerId)
        {
            return $"{baseClass} selected-row";
        }
        return baseClass;
    }

    private void AddCustomer()
    {
        _selectedCustomer = new CustomerJson
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
    
    private void EditCustomer()
    {
        if (_selectedCustomer == null)
            return;
    
        _showDialog = true;
        StateHasChanged();
    }

    private async Task OnDialogSubmit(CustomerJson customer)
    {
        _showDialog = false;
        await SaveEdit();
        
        StateHasChanged();
    }
    
    private void OnDialogCancel()
    {
        _editingCustomer = null;
        _showDialog = false;
        
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
            if (!string.IsNullOrEmpty(_editingCustomer.CustomerId))
            {
                // Update existing customer
                var updateResult = await CustomerService.UpdateCustomerAsync(_editingCustomer);
                updateResult.Match(
                    success =>
                    {
                        _ = LoadCustomers();
                        return true;
                    },
                    error =>
                    {
                        _ = ShowError($"Error Update customer: {error.Message}");
                        return false;
                    });
            }
            else
            {
                // Create new customer
                var saveResult = await CustomerService.CreateCustomerAsync(_editingCustomer);
                saveResult.Match(
                    success =>
                    {
                        _ = LoadCustomers();
                        return true;
                    },
                    error =>
                    {
                        _ = ShowError($"Error creating customer: {error.Message}");
                        return false;
                    });
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

    private async Task DeleteCustomer()
    {
        if (_selectedCustomer == null || string.IsNullOrWhiteSpace(_selectedCustomer.CustomerId))
            return;

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

    private void Close()
    {
        Navigation.NavigateTo("/masterdata");
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
            MessagingService.Unsubscribe<ToolbarItemClicked>(HandleToolbarClickAsync);
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
