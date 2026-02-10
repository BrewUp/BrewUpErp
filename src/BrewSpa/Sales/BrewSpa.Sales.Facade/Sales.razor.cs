using Microsoft.AspNetCore.Components;

namespace BrewSpa.Sales.Facade;

public partial class Sales : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    protected void NavigateToSection(string section)
    {
        switch (section.ToLower())
        {
            case "orders":
                Navigation.NavigateTo("/sales/salesorders");
                break;

            case "summary":
                break;
            
            case "invoices":
                break;

            case "quotes":
                break;
            
            case "customers":
                break;

            case "returns":
                break;

            case "performance":
                break;

            case "activities":
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

    ~Sales()
    {
        Dispose(false);
    }
    #endregion
}