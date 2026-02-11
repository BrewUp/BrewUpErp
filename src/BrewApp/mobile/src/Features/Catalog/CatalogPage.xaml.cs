using System.Globalization;

namespace BrewApp.Mobile.Features.Catalog;

public partial class CatalogPage : ContentPage
{
    public CatalogPage(CatalogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load data when the page appears
        if (BindingContext is CatalogViewModel viewModel)
        {
            await viewModel.LoadBeersAsync();
        }
    }

    private async void OnSearchCompleted(object? sender, EventArgs e)
    {
        if (BindingContext is CatalogViewModel viewModel)
        {
            await viewModel.LoadBeersAsync();
        }
    }

    private async void OnReloadClicked(object? sender, EventArgs e)
    {
        if (BindingContext is CatalogViewModel viewModel)
        {
            await viewModel.LoadBeersAsync();
        }
    }
}

/// <summary>
/// Converter to invert boolean values for binding.
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}
