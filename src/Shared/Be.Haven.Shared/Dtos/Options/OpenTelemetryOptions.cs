namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Root options for configuring OpenTelemetry in the application.
/// Typically bound from the configuration section "OpenTelemetry".
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Tracing-related settings (sampling, exporters, endpoints, filters, etc.).
    /// Bind from "OpenTelemetry:Tracing".
    /// </summary>
    public OpenTelemetryTracingOptions TracingSettings { get; set; } = new();

    /// <summary>
    /// Instrumentation switches for different libraries (AspNetCore, HttpClient, SqlClient, ...).
    /// Bind from "OpenTelemetry:Ins" (Instrumentation).
    /// </summary>
    public OpenTelemetryInsOptions InstrumentationSettings { get; set; } = new();
}
