using Microsoft.AspNetCore.Components;

namespace BrewSpa.Layout;

public partial class NavMenu : ComponentBase, IDisposable
{
    protected bool CollapseNavMenu;

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