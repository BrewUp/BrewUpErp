using Microsoft.Maui.Controls;

namespace BrewApp.Module.MasterData.Views;

public class MasterDataPage : ContentPage
{
    public MasterDataPage()
    {
        Title = "Master Data";
        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Label
                {
                    Text = "Master Data",
                    FontSize = 28,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "This is the Master Data module landing page.",
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
