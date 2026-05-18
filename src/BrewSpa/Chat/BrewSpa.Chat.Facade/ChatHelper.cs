using Microsoft.Extensions.DependencyInjection;

namespace BrewSpa.Chat.Facade;

public static class ChatHelper
{
    public static IServiceCollection AddChatFacadeServices(this IServiceCollection services)
    {
        // Add any facade-specific services here if needed
        return services;
    }
}
