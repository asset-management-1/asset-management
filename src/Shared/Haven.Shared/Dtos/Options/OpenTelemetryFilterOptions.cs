namespace Haven.Shared.Dtos.Options;

/// <summary>
/// Filtering options for OpenTelemetry instrumentation.
/// Use path prefixes to exclude noisy endpoints from being traced/logged,
/// e.g., health checks or static assets.
/// </summary>
public class OpenTelemetryFilterOptions
{
    /// <summary>
    /// Server-side (ASP.NET Core) request path prefixes to ignore.
    /// Any incoming HTTP request whose <c>Request.Path</c> starts with one of these
    /// (case-insensitive match) will be filtered out from tracing.
    /// Example: <c>["/health", "/swagger", "/_framework", "/css", "/js"]</c>.
    /// Default: empty (no server filters).
    /// </summary>
    public string[] ServerFilterPrefixes { get; set; } = [];

    /// <summary>
    /// Client-side (HttpClient) request path prefixes to ignore.
    /// Any outgoing HTTP request whose <c>RequestUri.AbsolutePath</c> starts with one of these
    /// (case-insensitive match) will be filtered out from tracing.
    /// Example: <c>["/health", "/metrics"]</c>.
    /// Default: empty (no client filters).
    /// </summary>
    public string[] ClientFilterPrefixes { get; set; } = [];

    /// <summary>
    /// Client-side request hostnames to ignore for tracing.
    /// Any outgoing HTTP request with a hostname matching one of these
    /// (case-insensitive match) will be excluded from tracing.
    /// Example use cases might include external services or APIs that should not be traced.
    /// Default: empty (no hosts are ignored).
    /// </summary>
    public string[] ClientIgnoredHosts { get; set; } = [];
}
