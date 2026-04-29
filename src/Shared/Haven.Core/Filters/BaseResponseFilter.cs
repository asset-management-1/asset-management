namespace Haven.Core.Filters;

public sealed class BaseResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        // Handle only ObjectResult with a ResponseDto<T> payload
        if (context.Result is ObjectResult { Value: not null } obj)
        {
            var t = obj.Value.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ResponseDto<>))
            {
                var http = context.HttpContext;

                // Get or create Meta
                var metaProp = t.GetProperty(nameof(ResponseDto<object>.Meta));
                var meta = (metaProp?.GetValue(obj.Value) as MetaDetailDto) ?? new MetaDetailDto();

                // API version (if any) for this request
                var version = http.GetRequestedApiVersion();
                string versionData = null;
                if (version is not null)
                {
                    versionData = $"{version.MajorVersion}.{(version.MinorVersion ?? 0)}";
                }

                // Fill correlation/tracing/timestamps
                var currentActivity = Activity.Current;
                meta.RequestId = http.TraceIdentifier;
                meta.CorrelationId = http.Items[X_CORRELATION_ID].ToString();
                meta.Version = versionData;
                meta.SpanId = currentActivity?.SpanId.ToString();
                meta.TraceId = currentActivity?.TraceId.ToString();
                meta.RequestTimestamp = http.Items[REQUEST_TIMESTAMP] as DateTime?;
                meta.ResponseTimestamp = DateTime.UtcNow;

                // Apply back to response
                metaProp?.SetValue(obj.Value, meta);
            }
        }
        await next();
    }
}