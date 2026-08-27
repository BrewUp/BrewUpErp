using System.Diagnostics.CodeAnalysis;
using BrewUp.Knowledge.Facade;
using NetArchTest.Rules;

namespace BrewUp.Knowledge.Tests;

[ExcludeFromCodeCoverage]
public sealed class KnowledgeArchitectureTests
{
    [Fact]
    public void Knowledge_does_not_depend_on_sibling_module_internals()
    {
        var result = Types.InAssembly(typeof(KnowledgeFacadeHelper).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BrewUp.MasterData.Domain",
                "BrewUp.Sales.Domain",
                "BrewUp.Warehouse.Domain",
                "BrewUp.Purchases.Domain")
            .GetResult()
            .IsSuccessful;

        Assert.True(result);
    }
}
