namespace Be.Haven.Shared.Dtos.Options.Observability;

/// <summary>
/// Options for shared logging config (read from "Observability:Logging").
/// </summary>
public class LoggingOptions
{
    // Write logs to the console (useful in dev and containers).
    public bool EnableConsole { get; set; } = true;

    // Minimum log level (Information/Warning/Error/...).
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;

    /// <summary>
    /// Output template used for non-HTTP logs (background jobs, services, etc.).
    /// </summary>
    public string DefaultOutputTemplate { get; set; }

    /// <summary>
    /// Output template used for HTTP request logs (SerilogRequestLogging).
    /// </summary>
    public string HttpRequestOutputTemplate { get; set; }
}
