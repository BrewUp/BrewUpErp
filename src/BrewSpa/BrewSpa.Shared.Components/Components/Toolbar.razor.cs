using Blazor.Messaging;
using BrewSpa.Shared.Components.CustomTypes;
using BrewSpa.Shared.Components.Messages;
using Microsoft.AspNetCore.Components;

namespace BrewSpa.Shared.Components.Components;

public partial class Toolbar : ComponentBase, IDisposable
{
    [Inject]
    private IMessagingService MessagingService { get; set; } = null!;
    
    [Parameter]
    public CurrentContext CurrentContext { get; set; } = new("None");

    private async Task AddNewItem()
    {
        await MessagingService.Publish(new ToolbarItemClicked(CurrentContext, ToolbarButtons.AddNewItem));
    }
    
    private async Task SaveCurrentItem()
    {
        await MessagingService.Publish(new ToolbarItemClicked(CurrentContext, ToolbarButtons.SaveCurrentItem));
    }
    
    #region Dispose
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup resources if needed
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Toolbar()
    {
        Dispose(false);
    }
    #endregion
}