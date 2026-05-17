namespace Be.Haven.Core.Handlers;

/// <summary>
/// DelegatingHandler that propagates the current request Correlation ID to outbound HTTP calls.
/// </summary>
public class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates a new <see cref="CorrelationIdHandler"/>.
    /// </summary>
    /// <param name="httpContextAccessor">
    /// Provides access to the current <see cref="HttpContext"/> so the handler can read
    /// the Correlation ID stored in <see cref="HttpContext.Items"/>.
    /// </param>
    public CorrelationIdHandler(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Intercepts an outgoing HTTP request and injects the Correlation ID header when available.
    /// </summary>
    /// <param name="request">The outgoing <see cref="HttpRequestMessage"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>The <see cref="HttpResponseMessage"/> returned by the inner handler.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Try to read Correlation ID from HttpContext (preferred).
        var ctx = _httpContextAccessor.HttpContext;

        // Fallback to Activity tag (OpenTelemetry) when HttpContext is not available (e.g., background tasks).
        var correlationId =
            ctx?.Items[X_CORRELATION_ID]?.ToString()
            ?? Activity.Current?.GetBaggageItem(OTEL_CORRELATION_ID_TAG)
            ?? Activity.Current?.GetTagItem(OTEL_CORRELATION_ID_TAG)?.ToString();

        // Inject Correlation ID into outbound request if it exists and wasn't already set by the caller.
        if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(X_CORRELATION_ID))
            request.Headers.Add(X_CORRELATION_ID, correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}
