using System.Diagnostics.CodeAnalysis;
using BrewUp.Rest.Module;
using NetArchTest.Rules;
namespace BrewUp.Rest.Tests.Architecture;

[ExcludeFromCodeCoverage]
public class BrewUpArchitectureTests
{
    [Fact]
    public void Should_BrewUpArchitecture_BeCompliant()
    {
        var types = Types.InAssembly(typeof(IModule).Assembly);

        var forbiddenAssemblies = new List<string>
        {
            "BrewUp.MasterData.Domain",
            "BrewUp.MasterData.Infrastructure",
            "BrewUp.MasterData.ReadModel",
            "BrewUp.MasterData.SharedKernel",
            
            "BrewUp.Sales.Domain",
            "BrewUp.Sales.Infrastructure",
            "BrewUp.Sales.ReadModel",
            "BrewUp.Sales.SharedKernel",
            
            "BrewUp.Warehouse.Domain",
            "BrewUp.Warehouse.Infrastructure",
            "BrewUp.Warehouse.ReadModel",
            "BrewUp.Warehouse.SharedKernel"
        };
        
        var result = types
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenAssemblies.ToArray())
            .GetResult()
            .IsSuccessful;

        Assert.True(result);
    }
}