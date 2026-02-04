using BrewApp.Shell.ViewModels;

namespace BrewApp.Shell;

public partial class LandingPage : ContentPage
{
    public LandingPage()
    {
        InitializeComponent();
        BindingContext = new LandingPageViewModel();
    }
}
