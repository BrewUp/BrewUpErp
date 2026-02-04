using Microsoft.Maui.Controls;

namespace BrewApp.Module.Sales.Views;

public class SalesPage : ContentPage
{
    public SalesPage()
    {
        Title = "Sales";
        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Label
                {
                    Text = "Sales",
                    FontSize = 28,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "This is the Sales module landing page.",
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
