using Microsoft.AspNetCore.Components;

namespace BrewSpa.MasterData.Facade;

public partial class MasterData : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void NavigateToSection(string section)
    {
        switch (section.ToLower())
        {
            case "customers":
                Navigation.NavigateTo("/masterdata/customers");
                break;
            case "suppliers":
                // Navigation.NavigateTo("/masterdata/suppliers");
                Console.WriteLine("Suppliers management - Coming soon");
                break;
            case "beers":
                // Navigation.NavigateTo("/masterdata/beers");
                Console.WriteLine("Beer management - Coming soon");
                break;
            case "products":
                // Navigation.NavigateTo("/masterdata/products");
                Console.WriteLine("Product management - Coming soon");
                break;
            case "categories":
                // Navigation.NavigateTo("/masterdata/categories");
                Console.WriteLine("Category management - Coming soon");
                break;
            case "inventory":
                // Navigation.NavigateTo("/masterdata/inventory");
                Console.WriteLine("Inventory management - Coming soon");
                break;
            case "settings":
                // Navigation.NavigateTo("/masterdata/settings");
                Console.WriteLine("Settings - Coming soon");
                break;
            case "reports":
                // Navigation.NavigateTo("/masterdata/reports");
                Console.WriteLine("Reports - Coming soon");
                break;
            default:
                Console.WriteLine($"Unknown section: {section}");
                break;
        }
    }
    
    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~MasterData()
    {
        Dispose(false);
    }
    #endregion
}