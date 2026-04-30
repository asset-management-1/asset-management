namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Configuration options for enabling OpenTelemetry instrumentation
/// for various application components.
/// </summary>
public class OpenTelemetryInsOptions
{
    /// <summary>
    /// Options for ASP.NET Core instrumentation.
    /// </summary>
    public OpenTelemetryInsToggleOptions AspNetCore { get; set; } = new();

    /// <summary>
    /// Options for HttpClient instrumentation.
    /// </summary>
    public OpenTelemetryInsToggleOptions HttpClient { get; set; } = new();
}