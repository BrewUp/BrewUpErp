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
            "BrewUp.Sales.Domain",
            "BrewUp.Sales.Infrastructure",
            "BrewUp.Sales.ReadModel",
            "BrewUp.Sales.SharedKernel"
        };
        
        var result = types
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenAssemblies.ToArray())
            .GetResult()
            .IsSuccessful;

        Assert.True(result);
    }
}