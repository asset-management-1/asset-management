namespace Be.Haven.Shared.Dtos.Options.Observability;

/// <summary>
/// Tracing options for OpenTelemetry. Bind from "OpenTelemetry:Tracing".
/// Controls whether tracing is enabled and how spans are exported.
/// </summary>
public class OpenTelemetryTracingOptions
{
    /// <summary>
    /// Global switch to enable/disable tracing.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Export spans to the console (useful in local/dev).
    /// Default: false.
    /// </summary>
    public bool ExportToConsole { get; set; }

    /// <summary>
    /// Export spans via OTLP to a collector/agent (e.g., OTel Collector, Ops Agent).
    /// Default: false.
    /// </summary>
    public bool ExportToOtlp { get; set; }

    /// <summary>
    /// Filters for server/client instrumentation (ignore paths, hosts, methods, etc.).
    /// </summary>
    public OpenTelemetryFilterOptions Filter { get; set; } = new();
}
