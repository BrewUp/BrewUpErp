using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BrewApp.Mobile.Models;
using BrewApp.Mobile.Services.Api;
using BrewApp.Mobile.Services.Mapping;
using BrewApp.Mobile.Services.Ui;

namespace BrewApp.Mobile.Features.Sales;

/// <summary>
/// ViewModel for the Sales Overview page.
/// Manages sales summary data and period selection.
/// </summary>
public class SalesSummaryViewModel : INotifyPropertyChanged
{
    private readonly ISalesApiClient _salesApiClient;
    private readonly INotificationService _notificationService;

    private string _selectedPeriod = "today";
    private SalesSummary? _salesSummary;
    private bool _isLoading;
    private bool _hasError;

    public event PropertyChangedEventHandler? PropertyChanged;

    public SalesSummaryViewModel(ISalesApiClient salesApiClient, INotificationService notificationService)
    {
        _salesApiClient = salesApiClient;
        _notificationService = notificationService;

        LoadSummaryCommand = new Command(async () => await LoadSummaryAsync());
        Periods = new ObservableCollection<string> { "today", "week", "month", "year" };
    }

    /// <summary>
    /// Available period options.
    /// </summary>
    public ObservableCollection<string> Periods { get; }

    /// <summary>
    /// Currently selected period.
    /// </summary>
    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (_selectedPeriod != value)
            {
                _selectedPeriod = value;
                OnPropertyChanged();
                _ = LoadSummaryAsync(); // Auto-load when period changes
            }
        }
    }

    /// <summary>
    /// Current sales summary data.
    /// </summary>
    public SalesSummary? SalesSummary
    {
        get => _salesSummary;
        private set
        {
            _salesSummary = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasData));
        }
    }

    /// <summary>
    /// Indicates whether data is currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether the last load attempt resulted in an error.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        private set
        {
            _hasError = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether sales data is currently available.
    /// </summary>
    public bool HasData => SalesSummary != null && !HasError;

    /// <summary>
    /// Command to manually reload the sales summary.
    /// </summary>
    public ICommand LoadSummaryCommand { get; }

    /// <summary>
    /// Loads sales summary data for the selected period.
    /// </summary>
    public async Task LoadSummaryAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            HasError = false;
            
            var dto = await _salesApiClient.GetSalesSummaryAsync(SelectedPeriod);
            
            if (dto != null)
            {
                SalesSummary = dto.ToSalesSummary();
            }
            else
            {
                SalesSummary = null;
                await _notificationService.ShowToastAsync("No sales data available for this period.");
            }
        }
        catch (ApiException ex)
        {
            HasError = true;
            SalesSummary = null;
            await _notificationService.ShowErrorAsync("Error Loading Sales", $"Could not load sales data: {ex.Message}", "Retry");
        }
        catch (Exception ex)
        {
            HasError = true;
            SalesSummary = null;
            await _notificationService.ShowErrorAsync("Unexpected Error", $"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
