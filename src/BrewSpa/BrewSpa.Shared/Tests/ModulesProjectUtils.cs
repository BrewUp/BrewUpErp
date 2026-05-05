namespace BrewSpa.Shared.Tests;

public static class ModulesProjectUtils
{
  private static readonly string[] SolutionProjects = [
        "BrewSpa.Dashboards.Facade",
        "BrewSpa.Dashboards.Services",
        "BrewSpa.Dashboards.Tests",

        "BrewSpa.MasterData.Facade",
        "BrewSpa.MasterData.Services",
        "BrewSpa.MasterData.Tests",

        "BrewSpa.Sales.Facade",
        "BrewSpa.Sales.Services",
        "BrewSpa.Sales.Tests"
    ];

  public static IEnumerable<string> GetModuleProjects(bool includeFacadeProjects, IEnumerable<string> excludeModules)
  {
    return SolutionProjects
        .Where(project =>
            (includeFacadeProjects || !project.EndsWith(".Facade")) &&
            !excludeModules.Any(module => project.StartsWith($"BrewSpa.{module}.")));
  }
}
