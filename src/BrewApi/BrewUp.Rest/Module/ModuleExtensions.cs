namespace BrewUp.Rest.Module;

public static class ModuleExtensions
{
    private static IList<IModule> _registeredModules = new List<IModule>();

    public static WebApplicationBuilder RegisterModules(this WebApplicationBuilder builder)
    {
        DiscoverModules();
        foreach (var module in _registeredModules)
        {
            module.Register(builder);
        }

        return builder;
    }

    public static WebApplication ConfigureModules(this WebApplication app)
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