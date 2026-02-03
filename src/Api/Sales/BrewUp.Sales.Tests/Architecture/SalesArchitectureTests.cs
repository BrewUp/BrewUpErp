using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BrewUp.Sales.Facade;
using NetArchTest.Rules;

namespace BrewUp.Sales.Tests.Architecture;

[ExcludeFromCodeCoverage]
public class SalesArchitectureTests
{
    [Fact]
    public void Should_SalesArchitecture_BeCompliant()
    {
        var types = Types.InAssembly(typeof(SalesFacadeHelper).Assembly);

        var forbiddenAssemblies = new List<string>
        {
            "BrewUp.MasterData.Domain",
            "BrewUp.MasterData.Facade",
            "BrewUp.MasterData.Infrastructure",
            "BrewUp.MasterData.ReadModel",
            "BrewUp.MasterData.SharedKernel",
            
            "BrewUp.Warehouse.Domain",
            "BrewUp.Warehouse.Facade",
            "BrewUp.Warehouse.Infrastructure",
            "BrewUp.Warehouse.ReadModel",
            "BrewUp.Warehouse.SharedKernel",
            
            "BrewUp.Purchase.Domain",
            "BrewUp.Purchase.Facade",
            "BrewUp.Purchase.Infrastructure",
            "BrewUp.Purchase.ReadModel",
            "BrewUp.Purchase.SharedKernel"
        };
        
        var result = types
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenAssemblies.ToArray())
            .GetResult()
            .IsSuccessful;

        Assert.True(result);
    }
    
    [Fact]
    public void SalesProjects_Should_Having_Namespace_StartingWith_Sales()
    {
        var salesModulePath = Path.Combine(VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName, "Sales");
        var subFolders = Directory.GetDirectories(salesModulePath);

        var netVersion = Environment.Version;

        var salesAssemblies = (from folder in subFolders
            let binFolder = Path.Join(folder, "bin", "Debug", $"net{netVersion.Major}.{netVersion.Minor}")
            where Directory.Exists(binFolder)
            let files = Directory.GetFiles(binFolder)
            let folderArray = folder.Split(Path.DirectorySeparatorChar)
            select files.FirstOrDefault(f => f.EndsWith($"{folderArray[^1]}.dll"))
            into assemblyFilename
            where assemblyFilename != null && !assemblyFilename.Contains("Test")
            select Assembly.LoadFile(assemblyFilename)).ToList();
        
        var salesTypes = Types.InAssemblies(salesAssemblies)
            .That()
            .DoNotHaveNameStartingWith("<>")
            .And()
            .AreNotNested()
            .GetTypes();
        
        var typesWithCorrectNamespace = Types.InAssemblies(salesAssemblies)
            .That()
            .ResideInNamespaceStartingWith("BrewUp.Sales")
            .And()
            .AreNotNested()
            .GetTypes();
        
        // Find types with incorrect namespace (difference between the two sets)
        var salesTypeArray = salesTypes as Type[] ?? salesTypes.ToArray();
        var typesWithIncorrectNamespace = salesTypeArray.Except(typesWithCorrectNamespace).ToList();

        foreach (var type in typesWithIncorrectNamespace)
        {
            if (type.Namespace != null)
                Assert.Fail(
                    $"Namespace violation detected: {type.FullName} in assembly {type.Assembly.GetName().Name} should start " +
                    $"with 'BrewUp.Sales' but is in namespace '{type.Namespace}'");
        }
    }
    
    private static class VisualStudioProvider
    {
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string? currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.slnx").Any())
            {
                directory = directory.Parent;
            }
            return directory!
                   ?? throw new DirectoryNotFoundException("Solution directory not found. Make sure to run this test from a solution folder.");
        }
    }
}