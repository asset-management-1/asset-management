namespace Be.Haven.Core.Middleware;

public class CorrelationHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationHandlerMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // 1) Extract or generate correlation ID
        var correlationId = context.Request.Headers[X_CORRELATION_ID].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[X_CORRELATION_ID] = correlationId;
        }

        var requestTimestamp = DateTime.UtcNow;

        // 2) Store correlation ID and timestamp in HttpContext.Items
        context.Items[X_CORRELATION_ID] = correlationId;
        context.Items[REQUEST_TIMESTAMP] = requestTimestamp;

        // 3) Add correlation ID to OpenTelemetry activity
        var currentActivity = Activity.Current;
        
        if (currentActivity != null)
        {
            currentActivity.SetTag(OTEL_CORRELATION_ID_TAG, correlationId);
            currentActivity.AddBaggage(OTEL_CORRELATION_ID_TAG, correlationId);
            currentActivity.SetTag(OTEL_REQUEST_TIMESTAMP_TAG, requestTimestamp.ToString("O"));
        }

        // 4) Add correlation ID + request id to Serilog LogContext
        using (LogContext.PushProperty(GCP_REQUEST_ID_PROPERTY, context.TraceIdentifier))
        using (LogContext.PushProperty(CORRELATION_ID_PROPERTY, correlationId))
        {
            // 5) Add correlation ID to response headers
            context.Response.Headers[X_CORRELATION_ID] = correlationId;

            await _next(context);
        }
    }
}