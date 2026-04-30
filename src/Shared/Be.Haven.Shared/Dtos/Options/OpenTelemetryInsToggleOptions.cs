namespace Be.Haven.Shared.Dtos.Options;

/// <summary>
/// Options for toggling OpenTelemetry instrumentation.
/// </summary>
public sealed record OpenTelemetryInsToggleOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry instrumentation is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether exceptions should be recorded.
    /// </summary>
    public bool RecordException { get; set; }
}