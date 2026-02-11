using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BrewApp.Mobile.Models;
using BrewApp.Mobile.Services.Api;
using BrewApp.Mobile.Services.Mapping;
using BrewApp.Mobile.Services.Ui;

namespace BrewApp.Mobile.Features.Catalog;

/// <summary>
/// ViewModel for the Catalog (Beers) page.
/// Manages beer list, search, and filtering.
/// </summary>
public class CatalogViewModel : INotifyPropertyChanged
{
    private readonly ICatalogApiClient _catalogApiClient;
    private readonly INotificationService _notificationService;

    private string _searchTerm = string.Empty;
    private bool _isLoading;
    private bool _hasError;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CatalogViewModel(ICatalogApiClient catalogApiClient, INotificationService notificationService)
    {
        _catalogApiClient = catalogApiClient;
        _notificationService = notificationService;

        Beers = new ObservableCollection<Beer>();
        SearchCommand = new Command(async () => await LoadBeersAsync());
        ClearSearchCommand = new Command(async () =>
        {
            SearchTerm = string.Empty;
            await LoadBeersAsync();
        });
    }

    /// <summary>
    /// List of beers from the catalog.
    /// </summary>
    public ObservableCollection<Beer> Beers { get; }

    /// <summary>
    /// Current search term for filtering beers.
    /// </summary>
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm != value)
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
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
    /// Indicates whether beer data is currently available.
    /// </summary>
    public bool HasData => Beers.Count > 0 && !HasError;

    /// <summary>
    /// Command to execute search based on SearchTerm.
    /// </summary>
    public ICommand SearchCommand { get; }

    /// <summary>
    /// Command to clear search and reload all beers.
    /// </summary>
    public ICommand ClearSearchCommand { get; }

    /// <summary>
    /// Loads beers from the catalog API with optional search filtering.
    /// </summary>
    public async Task LoadBeersAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            HasError = false;

            var searchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm;
            var dtos = await _catalogApiClient.GetBeersAsync(searchTerm);

            Beers.Clear();
            if (dtos != null && dtos.Count > 0)
            {
                foreach (var dto in dtos)
                {
                    Beers.Add(dto.ToBeer());
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    await _notificationService.ShowToastAsync($"No beers found matching '{searchTerm}'.");
                }
                else
                {
                    await _notificationService.ShowToastAsync("No beers available in the catalog.");
                }
            }
        }
        catch (ApiException ex)
        {
            HasError = true;
            Beers.Clear();
            await _notificationService.ShowErrorAsync("Error Loading Catalog", $"Could not load beers: {ex.Message}", "Retry");
        }
        catch (Exception ex)
        {
            HasError = true;
            Beers.Clear();
            await _notificationService.ShowErrorAsync("Unexpected Error", $"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasData));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
