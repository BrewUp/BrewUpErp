using System.Diagnostics;
using System.Reflection;

namespace BrewUp.Knowledge.Core.Wiki;

public static class KnowledgeWikiTelemetry
{
    public const string SourceName = "BrewUp.Knowledge.Wiki";

    private static readonly string SourceVersion =
        typeof(KnowledgeWikiTelemetry).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "1.0.0";

    public static readonly ActivitySource Source = new(SourceName, SourceVersion);
}

