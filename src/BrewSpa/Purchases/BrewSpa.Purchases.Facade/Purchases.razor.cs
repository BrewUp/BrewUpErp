using Microsoft.AspNetCore.Components;

namespace BrewSpa.Purchases.Facade;

public partial class Purchases : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void NavigateToSection(string section)
    {
        if (section == "orders")
            Navigation.NavigateTo("/purchases/orders");
    }
}
