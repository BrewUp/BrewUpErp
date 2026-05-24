using BrewSpa.Chat.Application.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Chat.Application.Extensions;

public static class ChatApplicationHelper
{
    public static IServiceCollection AddChatServices(this IServiceCollection services,
        WebAssemblyHostConfiguration configurationManager)
    {
        services.AddHttpClient<IChatService, ChatService>(client =>
        {
            client.BaseAddress = new Uri(configurationManager["BrewApi:ChatApiBaseAddress"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        return services;
    }
}
