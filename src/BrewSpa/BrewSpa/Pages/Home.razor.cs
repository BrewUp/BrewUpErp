using BitBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.Pages;

public partial class Home : ComponentBase, IDisposable
{
    [CascadingParameter]
    BitCard Parent { get; set; } = null!;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    // protected override void OnInitialized() => Parent.NotifyHasImageChanged(hasImage: true);
    
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

    ~Home()
    {
        Dispose(false);
    }
    #endregion
}