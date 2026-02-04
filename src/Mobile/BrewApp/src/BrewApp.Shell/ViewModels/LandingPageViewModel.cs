using CommunityToolkit.Mvvm.ComponentModel;

namespace BrewApp.Shell.ViewModels;

public partial class LandingPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "BrewUp ERP Mobile";

    [ObservableProperty]
    private string subtitle = "Manage your brewery: Master Data, Sales, Warehouse, Purchase";
}
