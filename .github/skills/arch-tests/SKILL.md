---
name: arch-tests
description: >
  Generate the Architecture test class for a new BrewUp module.
  Produces `<Module>ArchitectureTests.cs` inside `<Module>/BrewUp.<Module>.Tests/Architecture/`.
  Use whenever a new module folder has been scaffolded and the architecture guard-tests are still missing.
  WHEN: "generate architecture tests", "add arch tests for <Module>", "create architecture test", "scaffold arch tests".
applyTo: "**"
---

# BrewUp Architecture-Tests Skill

## Purpose
Generate a complete, ready-to-compile `<Module>ArchitectureTests.cs` that enforces the three standard rules
every BrewUp module must satisfy:

1. **No cross-module dependency** – the Facade assembly must not reference any other module's projects.
2. **Namespace compliance** – every type in every module project must live in a `BrewUp.<Module>.*` namespace.
3. **Domain isolation** – the Domain assembly must not depend on outer layers (Infrastructure, ReadModel, Facade).

---

## Inputs you must collect before generating

| Input | How to obtain it |
|---|---|
| `MODULE` | The PascalCase module name (e.g. `Sales`, `Warehouse`, `Dashboards`, `Inventory`). Ask the user if not stated. |
| `MODULE_FOLDER` | The top-level solution sub-folder that contains the module projects (usually equals `MODULE`). Verify with `list_dir` on the solution root if uncertain. |
| `FACADE_HELPER` | The class used as the anchor type for `Types.InAssembly(...)`. Convention: `<Module>FacadeHelper`. Confirm by searching `*FacadeHelper*` inside `<MODULE_FOLDER>/BrewUp.<MODULE>.Facade/`. |
| `DOMAIN_HELPER` | The class used as the anchor type for the Domain-isolation test. Convention: `<Module>DomainHelper` (lives in `BrewUp.<Module>.Domain`). Check with `grep_search`. |
| `USING_DOMAIN` | `true` if a `DomainHelper` type exists and the Domain-isolation test should be emitted; `false` otherwise. |

---

## Steps

1. **Collect inputs** (search the workspace; ask the user only if a value cannot be determined).
2. **Determine the output path**:
   ```
   <solution-root>/<MODULE_FOLDER>/BrewUp.<MODULE>.Tests/Architecture/<MODULE>ArchitectureTests.cs
   ```
3. **Generate the file** using the template below, substituting all placeholders.
4. **Verify** the target `.csproj` already references:
   - `BrewUp.Shared.Tests`
   - `BrewUp.<MODULE>.Facade`
   - `BrewUp.<MODULE>.Domain` (when `USING_DOMAIN` is true)
   - NuGet packages `NetArchTest.Rules` and `xunit`
   If any reference is missing, show the `<ProjectReference>` or `<PackageReference>` XML the user must add and explain where.

---

## Code template

```csharp
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BrewUp.{{MODULE}}.Facade;
{{USING_DOMAIN_IMPORT}}
using BrewUp.Shared.Tests;
using NetArchTest.Rules;

namespace BrewUp.{{MODULE}}.Tests.Architecture;

[ExcludeFromCodeCoverage]
public class {{MODULE}}ArchitectureTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // 1. Cross-module dependency guard
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Should_{{MODULE}}Architecture_BeCompliant()
    {
        var types = Types.InAssembly(typeof({{FACADE_HELPER}}).Assembly);

        var forbiddenAssemblies = ModulesProjectUtils.GetModuleProjects(true, ["{{MODULE}}"]);

        var result = types
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenAssemblies.ToArray())
            .GetResult()
            .IsSuccessful;

        Assert.True(result);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 2. Namespace compliance
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public void {{MODULE}}Projects_Should_Having_Namespace_StartingWith_{{MODULE}}()
    {
        var modulePath = Path.Combine(VisualStudioProvider.TryGetSolutionDirectoryInfo().FullName, "{{MODULE_FOLDER}}");
        var subFolders = Directory.GetDirectories(modulePath);

        var netVersion = Environment.Version;

        var moduleAssemblies = (from folder in subFolders
            let binFolder = Path.Join(folder, "bin", "Debug", $"net{netVersion.Major}.{netVersion.Minor}")
            where Directory.Exists(binFolder)
            let files = Directory.GetFiles(binFolder)
            let folderArray = folder.Split(Path.DirectorySeparatorChar)
            select files.FirstOrDefault(f => f.EndsWith($"{folderArray[^1]}.dll"))
            into assemblyFilename
            where assemblyFilename != null && !assemblyFilename.Contains("Test")
            select Assembly.LoadFile(assemblyFilename)).ToList();

        var moduleTypes = Types.InAssemblies(moduleAssemblies)
            .That()
            .DoNotHaveNameStartingWith("<>")
            .And()
            .AreNotNested()
            .GetTypes();

        var typesWithCorrectNamespace = Types.InAssemblies(moduleAssemblies)
            .That()
            .ResideInNamespaceStartingWith("BrewUp.{{MODULE}}")
            .And()
            .AreNotNested()
            .GetTypes();

        var moduleTypeArray = moduleTypes as Type[] ?? moduleTypes.ToArray();
        var typesWithIncorrectNamespace = moduleTypeArray.Except(typesWithCorrectNamespace).ToList();

        foreach (var type in typesWithIncorrectNamespace)
        {
            if (type.Namespace != null)
                Assert.Fail(
                    $"Namespace violation detected: {type.FullName} in assembly {type.Assembly.GetName().Name} " +
                    $"should start with 'BrewUp.{{MODULE}}' but is in namespace '{type.Namespace}'");
        }
    }

{{DOMAIN_ISOLATION_TEST}}
    // ──────────────────────────────────────────────────────────────────────────
    // Infrastructure
    // ──────────────────────────────────────────────────────────────────────────
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
                   ?? throw new DirectoryNotFoundException(
                       "Solution directory not found. Make sure to run this test from a solution folder.");
        }
    }
}
```

### `{{DOMAIN_ISOLATION_TEST}}` block (emit only when `USING_DOMAIN` is true)

```csharp
    // ──────────────────────────────────────────────────────────────────────────
    // 3. Domain isolation
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(typeof({{DOMAIN_HELPER}}).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "BrewUp.Infrastructure",
                "BrewUp.{{MODULE}}.Infrastructure",
                "BrewUp.{{MODULE}}.ReadModel",
                "BrewUp.{{MODULE}}.Facade")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

```

---

## Placeholder reference

| Placeholder | Replaced with |
|---|---|
| `{{MODULE}}` | PascalCase module name, e.g. `Sales` |
| `{{MODULE_FOLDER}}` | Top-level folder name, e.g. `Sales` (may differ if folder name ≠ module name) |
| `{{FACADE_HELPER}}` | e.g. `SalesFacadeHelper` |
| `{{DOMAIN_HELPER}}` | e.g. `DomainHelper` (the concrete class, not the interface) |
| `{{USING_DOMAIN_IMPORT}}` | `using BrewUp.{{MODULE}}.Domain;` when `USING_DOMAIN` is true, empty otherwise |
| `{{DOMAIN_ISOLATION_TEST}}` | The full Domain isolation `[Fact]` block when `USING_DOMAIN` is true, empty otherwise |

---

## After file creation

- Tell the user the exact path of the created file.
- Remind them to add the file to source control if the project uses explicit `<Compile>` includes.
- If any `.csproj` references were missing, present the XML snippets needed.
- Suggest running `dotnet test --filter "FullyQualifiedName~Architecture"` to confirm the tests compile and pass.
