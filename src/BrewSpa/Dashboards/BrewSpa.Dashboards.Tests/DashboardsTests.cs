using BrewSpa.Dashboards.Facade;
using BrewSpa.Shared.Tests;
using NetArchTest.Rules;

namespace BrewSpa.Dashboards.Tests
{
  public class DashboardsTests
  {
    [Fact]
    public void Should_DashboardsArchitecture_BeCompliant()
    {
      var types = Types.InAssembly(typeof(DashboardsHelper).Assembly);

      var forbiddenAssemblies = ModulesProjectUtils.GetModuleProjects(true, ["Dashboards"]);

      var result = types
          .ShouldNot()
          .HaveDependencyOnAny(forbiddenAssemblies.ToArray())
          .GetResult()
          .IsSuccessful;

      Assert.True(result);
    }
  }
}
