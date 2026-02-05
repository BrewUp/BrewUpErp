using Microsoft.Maui.Controls;

namespace BrewApp.Module.Warehouse.Views;

public class WarehousePage : ContentPage
{
    public WarehousePage()
    {
        Title = "Warehouse";
        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Label
                {
                    Text = "Warehouse",
                    FontSize = 28,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "This is the Warehouse module landing page.",
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
