using Microsoft.AspNetCore.Components;

namespace BrewSpa.Layout;

public partial class NavMenu : ComponentBase, IDisposable
{
    protected bool CollapseNavMenu;

  [Inject]
  private IConfiguration Configuration { get; set; } = null!;

  protected bool IsDataMasterEnabled { get; set; }
  protected bool IsSalesEnabled { get; set; }

  protected override void OnInitialized()
  {
    IsDataMasterEnabled = Configuration!.GetValue<bool>("Modules:EnableDataMaster");
    IsSalesEnabled = Configuration!.GetValue<bool>("Modules:EnableSales");
  }

  private string? NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        CollapseNavMenu = !CollapseNavMenu;
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

    ~NavMenu()
    {
        Dispose(false);
    }
    #endregion
}