using Blazor.Messaging;
using BrewSpa.Shared.Components.CustomTypes;
using BrewSpa.Shared.Components.Messages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BrewSpa.Sales.Facade.Components.Orders;

public partial class SalesOrders : ComponentBase, IDisposable
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IMessagingService MessagingService { get; set; } = null!;
    
    private readonly CurrentContext _currentContext = new ("SalesOrders");
    
    protected override async Task OnInitializedAsync()
    {
        MessagingService.Subscribe<ToolbarItemClicked>(HandleToolbarClickAsync);
     
    }
    
    private async Task HandleToolbarClickAsync(ToolbarItemClicked message)
    {
        if (message.CurrentContext != _currentContext) return;

        switch (message.ToolbarButton.Name)
        {
            case nameof(ToolbarButtons.AddNewItem):
                break;
            
            case nameof(ToolbarButtons.Refresh):
                await RefreshData();
                break;
            
            case nameof(ToolbarButtons.Close):
                Close();
                break;
        }
    }
    
    private async Task RefreshData()
    {
        Console.WriteLine("RefreshData button clicked");
     
    }
    
    private void Close()
    {
        Navigation.NavigateTo("/sales");
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

    ~SalesOrders()
    {
        Dispose(false);
    }
    #endregion   
}