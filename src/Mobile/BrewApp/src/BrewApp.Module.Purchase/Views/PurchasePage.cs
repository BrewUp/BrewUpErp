using Microsoft.Maui.Controls;

namespace BrewApp.Module.Purchase.Views;

public class PurchasePage : ContentPage
{
    public PurchasePage()
    {
        Title = "Purchase";
        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Label
                {
                    Text = "Purchase",
                    FontSize = 28,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "This is the Purchase module landing page.",
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
