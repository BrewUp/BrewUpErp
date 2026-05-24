using Microsoft.AspNetCore.SignalR;

namespace BrewUp.Mother.Hubs;

public class MotherHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("MotherHubConnected", "BrewUp Mother is Connected").ConfigureAwait(false);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("MotherHubDisconnected", "BrewUp Mother Disconnected").ConfigureAwait(false);

        await base.OnDisconnectedAsync(exception);
    }
}