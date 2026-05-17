namespace Be.Haven.Core.Helpers;

public static class OpenTelemetryHelper
{
    /// <summary>
    /// Configures the OpenTelemetry settings for tracing and metrics collection
    /// based on the provided configurations and resource builder.
    /// </summary>
    /// <param name="builder">
    /// The OpenTelemetryBuilder used to configure tracing and metrics providers.
    /// </param>
    /// <param name="resource">
    /// The ResourceBuilder specifying the resource details for the OpenTelemetry setup.
    /// </param>
    /// <param name="otel">
    /// The OpenTelemetryOptions containing the tracing and instrumentation configuration settings.
    /// </param>
    public static void Configure(
        OpenTelemetryBuilder builder,
        ResourceBuilder resource,
        OpenTelemetryOptions otel)
    {
        var filter = otel.TracingSettings.Filter;
        var serverPrefixes = filter.ServerFilterPrefixes ?? [];
        var clientPrefixes = filter.ClientFilterPrefixes ?? [];
        var ignoredHosts = filter.ClientIgnoredHosts ?? [];

        // Configure tracing instrumentation and exporters for the current service.
        builder.WithTracing(b =>
        {
            b.AddSource(BACKGROUND_JOBS);
            b.AddQuartzInstrumentation();
            b.SetResourceBuilder(resource);
            AddAspNetCore(b, otel, serverPrefixes);
            AddHttpClient(b, otel, clientPrefixes, ignoredHosts);
            AddTraceExporters(b, otel.TracingSettings);
        });
        // .WithMetrics(m =>
        // {
        //     m.SetResourceBuilder(resource);
        //     m.AddHttpClientInstrumentation();
        //     m.AddRuntimeInstrumentation();
        //     AddMetricExporters(m, otel.TracingSettings);
        // });
    }

    /// <summary>
    /// Adds ASP.NET Core instrumentation to the TracerProviderBuilder
    /// with specific filtering and configuration based on OpenTelemetry options.
    /// </summary>
    /// <param name="b">
    /// The TracerProviderBuilder used to configure tracing instrumentation providers.
    /// </param>
    /// <param name="otel">
    /// The OpenTelemetryOptions containing the instrumentation and tracing configuration settings.
    /// </param>
    /// <param name="serverPrefixes">
    /// An array of server path prefixes to be excluded from tracing instrumentation.
    /// </param>
    private static void AddAspNetCore(
        TracerProviderBuilder b,
        OpenTelemetryOptions otel,
        string[] serverPrefixes)
    {
        if (!otel.InstrumentationSettings.AspNetCore.Enabled) return;

        b.AddAspNetCoreInstrumentation(o =>
        {
            o.RecordException = otel.InstrumentationSettings.AspNetCore.RecordException;
            o.Filter = ctx => ShouldTraceServerRequest(ctx, serverPrefixes);
        });
    }

    /// <summary>
    /// Configures the HTTP client instrumentation for tracing, applying filters and settings
    /// based on the provided OpenTelemetry configuration.
    /// </summary>
    /// <param name="b">
    /// The TracerProviderBuilder used to configure tracing instrumentation for HTTP client requests.
    /// </param>
    /// <param name="otel">
    /// The OpenTelemetryOptions containing the instrumentation and tracing configuration settings.
    /// </param>
    /// <param name="clientPrefixes">
    /// The array of string prefixes used to filter out certain HTTP client requests based on their paths.
    /// </param>
    /// <param name="ignoredHosts">
    /// The array of host names to be ignored during HTTP client instrumentation.
    /// </param>
    private static void AddHttpClient(
        TracerProviderBuilder b,
        OpenTelemetryOptions otel,
        string[] clientPrefixes,
        string[] ignoredHosts)
    {
        if (!otel.InstrumentationSettings.HttpClient.Enabled) return;

        b.AddHttpClientInstrumentation(o =>
        {
            o.RecordException = otel.InstrumentationSettings.HttpClient.RecordException;
            o.FilterHttpRequestMessage = req => ShouldTraceHttpRequest(req, clientPrefixes, ignoredHosts);
        });
    }

    /// <summary>
    /// Adds trace exporters to the TracerProviderBuilder based on the provided tracing options.
    /// </summary>
    /// <param name="b">
    /// The TracerProviderBuilder used for configuring tracing settings and adding exporters.
    /// </param>
    /// <param name="tracing">
    /// The OpenTelemetryTracingOptions specifying which trace exporters should be enabled.
    /// </param>
    private static void AddTraceExporters(TracerProviderBuilder b, OpenTelemetryTracingOptions tracing)
    {
        if (tracing.ExportToConsole) b.AddConsoleExporter();
        if (tracing.ExportToOtlp) b.AddOtlpExporter();
    }

    /// <summary>
    /// Determines whether an incoming HTTP request should be traced.
    /// </summary>
    /// <param name="context">The current ASP.NET Core HTTP context.</param>
    /// <param name="serverPrefixes">The path prefixes excluded from tracing.</param>
    /// <returns><c>true</c> when the request should be traced; otherwise <c>false</c>.</returns>
    private static bool ShouldTraceServerRequest(
        HttpContext context,
        string[] serverPrefixes)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        return !StartsWithAny(path, serverPrefixes);
    }

    /// <summary>
    /// Determines whether an outgoing HTTP request should be traced.
    /// </summary>
    /// <param name="request">The outgoing HTTP request.</param>
    /// <param name="clientPrefixes">The path prefixes excluded from tracing.</param>
    /// <param name="ignoredHosts">The hosts excluded from tracing.</param>
    /// <returns><c>true</c> when the request should be traced; otherwise <c>false</c>.</returns>
    private static bool ShouldTraceHttpRequest(
        HttpRequestMessage request,
        string[] clientPrefixes,
        string[] ignoredHosts)
    {
        if (IsIgnoredHost(request.RequestUri, ignoredHosts))
        {
            return false;
        }

        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

        return !StartsWithAny(path, clientPrefixes);
    }

    /// <summary>
    /// Determines if the specified string starts with any of the provided prefixes,
    /// using a case-insensitive comparison.
    /// </summary>
    /// <param name="value">
    /// The string to be checked against the prefixes.
    /// </param>
    /// <param name="prefixes">
    /// An array of prefixes to compare with the beginning of the string.
    /// </param>
    /// <returns>
    /// True if the string starts with any of the provided prefixes; otherwise, false.
    /// </returns>
    private static bool StartsWithAny(string value, string[] prefixes) =>
        !string.IsNullOrEmpty(value) &&
        prefixes.Any(pre => !string.IsNullOrWhiteSpace(pre) &&
                            value.AsSpan().StartsWith(pre, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Determines if the provided URI matches any of the specified ignored hosts.
    /// </summary>
    /// <param name="uri">
    /// The URI to evaluate for a match against the ignored hosts.
    /// </param>
    /// <param name="hosts">
    /// An array of host strings that are to be ignored.
    /// </param>
    /// <returns>
    /// True if the URI matches any of the ignored hosts; otherwise, false.
    /// </returns>
    private static bool IsIgnoredHost(Uri uri, string[] hosts)
    {
        if (uri is null) return false;
        var hostWithPort = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
        return hosts.Contains(hostWithPort, StringComparer.OrdinalIgnoreCase) ||
               hosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);
    }
}
