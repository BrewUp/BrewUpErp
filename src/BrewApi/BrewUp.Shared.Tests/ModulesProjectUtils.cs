namespace BrewUp.Shared.Tests;

public static class ModulesProjectUtils
{
    private static readonly string[] SolutionProjects = [
        "BrewUp.MasterData.Domain",
        "BrewUp.MasterData.Facade",
        "BrewUp.MasterData.Infrastructure",
        "BrewUp.MasterData.ReadModel", 
        "BrewUp.MasterData.SharedKernel",
        "BrewUp.MasterData.Tests",
        
        "BrewUp.Sales.Domain",
        "BrewUp.Sales.Facade",
        "BrewUp.Sales.Infrastructure",
        "BrewUp.Sales.ReadModel", 
        "BrewUp.Sales.SharedKernel",
        "BrewUp.Sales.Tests",
        
        "BrewUp.Warehouse.Domain",
        "BrewUp.Warehouse.Facade",
        "BrewUp.Warehouse.Infrastructure",
        "BrewUp.Warehouse.ReadModel", 
        "BrewUp.Warehouse.SharedKernel",
        "BrewUp.Warehouse.Tests",
        
        "BrewUp.Dashboards.Domain",
        "BrewUp.Dashboards.Entities",
        "BrewUp.Dashboards.Facade",
        "BrewUp.Dashboards.Infrastructure",
        "BrewUp.Dashboards.ReadModel", 
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