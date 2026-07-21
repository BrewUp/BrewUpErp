using System.Diagnostics;
using System.Reflection;

namespace BrewUp.Mother.Facade.Telemetry;

/// <summary>
/// Central telemetry definitions for the Mother coordination pipeline.
/// The <see cref="Source"/> is used to emit distributed tracing spans for every
/// non-deterministic coordination step, so the whole orchestration can be reconstructed.
/// </summary>
public static class MotherTelemetry
{
    /// <summary>
    /// Name of the <see cref="ActivitySource"/>. Register this name with the tracer provider
    /// (e.g. <c>AddSource(MotherTelemetry.SourceName)</c>) to collect Mother coordination spans.
    /// </summary>
    public const string SourceName = "BrewUp.Mother.Coordinator";

    private static readonly string SourceVersion =
        typeof(MotherTelemetry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "1.0.0";

    /// <summary>
    /// The activity source used to create coordination spans.
    /// </summary>
    public static readonly ActivitySource Source = new(SourceName, SourceVersion);
}
