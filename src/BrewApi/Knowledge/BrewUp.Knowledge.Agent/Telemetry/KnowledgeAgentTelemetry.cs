using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace BrewUp.Knowledge.Agent.Telemetry;

public static class KnowledgeAgentTelemetry
{
    public const string SourceName = "BrewUp.Knowledge.Agent";
    public const string MeterName = "BrewUp.Agent";

    private static readonly string SourceVersion =
        typeof(KnowledgeAgentTelemetry).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "1.0.0";

    public static readonly ActivitySource Source = new(SourceName, SourceVersion);
    public static readonly Meter Meter = new(MeterName, SourceVersion);
    public static readonly Counter<long> ToolCalls = Meter.CreateCounter<long>("brewup.agent.tool.calls");
}
