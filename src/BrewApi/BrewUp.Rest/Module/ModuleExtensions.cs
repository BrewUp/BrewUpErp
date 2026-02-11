namespace BrewUp.Rest.Module;

internal static class ModuleExtensions
{
    private static IList<IModule> _registeredModules = new List<IModule>();

    internal static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        DiscoverModules();
        foreach (var module in _registeredModules)
        {
            module.Register(builder);
        }

        return builder;
    }

    internal static WebApplication ConfigureModules(this WebApplication app)
    {
        foreach (var module in _registeredModules)
        {
            module.Configure(app);
        }

        return app;
    }

    private static void DiscoverModules()
    {
        var modules =  typeof(IModule).Assembly
            .GetTypes()
            .Where(p => p.IsClass && p.IsAssignableTo(typeof(IModule)))
            .Select(Activator.CreateInstance)
            .Cast<IModule>();

        _registeredModules = modules
            .Where(m => m.IsEnabled)
            .OrderBy(m => m.Order)
            .ToList();
    }
}