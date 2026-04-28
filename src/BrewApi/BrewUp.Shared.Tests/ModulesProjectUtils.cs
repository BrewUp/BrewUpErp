namespace BrewUp.Shared.Tests;

public static class ModulesProjectUtils
{
    private static readonly string[] SolutionProjects = [
        "BrewUp.MasterData.Domain",
        "BrewUp.MasterData.Facade",
        "BrewUp.MasterData.Infrastructure",
        "BrewUp.MasterData.readModel", 
        "BrewUp.MasterData.SharedKernel",
        "BrewUp.MasterData.Tests",
        
        "BrewUp.Sales.Domain",
        "BrewUp.Sales.Facade",
        "BrewUp.Sales.Infrastructure",
        "BrewUp.Sales.readModel", 
        "BrewUp.Sales.SharedKernel",
        "BrewUp.Sales.Tests",
        
        "BrewUp.Warehouse.Domain",
        "BrewUp.Warehouse.Facade",
        "BrewUp.Warehouse.Infrastructure",
        "BrewUp.Warehouse.readModel", 
        "BrewUp.Warehouse.SharedKernel",
        "BrewUp.Warehouse.Tests",
        
        "BrewUp.Dashboards.Domain",
        "BrewUp.Dashboards.Facade",
        "BrewUp.Dashboards.Infrastructure",
        "BrewUp.Dashboards.readModel", 
        "BrewUp.Dashboards.SharedKernel",
        "BrewUp.Dashboards.Tests"
    ];

    public static IEnumerable<string> GetModuleProjects(bool includeFacadeProjects, IEnumerable<string> excludeModules)
    {
        return SolutionProjects
            .Where(project =>
                (includeFacadeProjects || !project.EndsWith(".Facade")) &&
                !excludeModules.Any(module => project.StartsWith($"BrewUp.{module}.")));
    }
}