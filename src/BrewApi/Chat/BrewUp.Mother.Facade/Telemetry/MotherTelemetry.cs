using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace BrewUp.Mother.Facade.Telemetry;

/// <summary>
/// Central telemetry definitions for the Mother coordination pipeline.
/// The <see cref="Source"/> is used to emit distributed tracing spans for every
/// semantic coordination step, so the whole orchestration can be reconstructed.
/// </summary>
public static class MotherTelemetry
{
    /// <summary>
    /// Name of the <see cref="ActivitySource"/>. Register this name with the tracer provider
    /// (e.g. <c>AddSource(MotherTelemetry.SourceName)</c>) to collect Mother coordination spans.
    /// </summary>
    public const string SourceName = "BrewUp.Mother.Coordinator";
    public const string MeterName = "BrewUp.Agent";

    private static readonly string SourceVersion =
        typeof(MotherTelemetry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "1.0.0";

    /// <summary>
    /// The activity source used to create coordination spans.
    /// </summary>
    public static readonly ActivitySource Source = new(SourceName, SourceVersion);
    public static readonly Meter Meter = new(MeterName, SourceVersion);
    public static readonly Counter<long> AgentRuns = Meter.CreateCounter<long>("brewup.agent.runs");
    public static readonly Counter<long> AgentHandoffs = Meter.CreateCounter<long>("brewup.agent.handoffs");
    public static readonly Histogram<double> AgentRunDuration = Meter.CreateHistogram<double>(
        "brewup.agent.run.duration",
        unit: "s");
}
