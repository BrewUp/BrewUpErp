using BrewUp.Mother.Facade.Telemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BrewUp.Rest.Module;

/// <summary>
/// Telemetry Module for configuring OpenTelemetry tracing and metrics.
/// Emits distributed traces for the whole Mother coordination pipeline (every delegated agent step)
/// as well as ASP.NET Core, HttpClient and SqlClient activity.
/// </summary>
public class TelemetryModule : IModule
{
    /// <summary>
    /// Well-known ActivitySource used by the Microsoft.Extensions.AI chat client (see AddBrewUpChat).
    /// </summary>
    private const string ChatSourceName = "BrewUp.Chat";

    /// <summary>
    /// Indicates whether the module is enabled and should be registered in the application.
    /// </summary>
    public bool IsEnabled => true;

    /// <summary>
    /// Set the order in which the module should be registered in the application.
    /// Registered early so instrumentation wraps the rest of the pipeline.
    /// </summary>
    public int Order => 0;

    /// <summary>
    /// Registers OpenTelemetry tracing and metrics providers.
    /// </summary>
    public IServiceCollection Register(WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "BrewUp.Rest";
        var useConsoleExporter = builder.Configuration.GetValue(
            "OpenTelemetry:UseConsoleExporter",
            builder.Environment.IsDevelopment());

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: "1.0.0");

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(MotherTelemetry.SourceName)
                    .AddSource(ChatSourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();

                if (useConsoleExporter)
                    tracing.AddConsoleExporter();

                // OTLP export is configured centrally by ServiceDefaults via UseOtlpExporter()
                // (reads OTEL_EXPORTER_OTLP_ENDPOINT, injected by .NET Aspire). Signal-specific
                // AddOtlpExporter must NOT be mixed with the cross-cutting UseOtlpExporter.
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(MotherTelemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                // OTLP export is configured centrally by ServiceDefaults via UseOtlpExporter().
            });

        return builder.Services;
    }

    /// <summary>
    /// No middleware to configure for this module.
    /// </summary>
    public WebApplication Configure(WebApplication app) => app;
}
