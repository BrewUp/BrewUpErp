using System.Globalization;

namespace BrewApp.Mobile.Features.Sales;

public partial class SalesPage : ContentPage
{
    public SalesPage(SalesSummaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load data when the page appears
        if (BindingContext is SalesSummaryViewModel viewModel)
        {
            await viewModel.LoadSummaryAsync();
        }
    }

    private async void OnReloadClicked(object? sender, EventArgs e)
    {
        if (BindingContext is SalesSummaryViewModel viewModel)
        {
            await viewModel.LoadSummaryAsync();
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
