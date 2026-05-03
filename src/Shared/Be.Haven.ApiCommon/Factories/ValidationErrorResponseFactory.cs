namespace Be.Haven.ApiCommon.Factories;

/// <summary>
/// Builds a unified <see cref="ResponseDto{T}"/> payload for ASP.NET Core ModelState validation errors,
/// so all controllers return a consistent error envelope.
/// </summary>
public static class ValidationErrorResponseFactory
{
    /// <summary>
    /// Creates a 400 Bad Request response from the current <see cref="ActionContext"/> when model binding/validation fails.
    /// </summary>
    /// <param name="context">The current action context containing <see cref="ModelStateDictionary"/> and HTTP context.</param>
    /// <returns>
    /// An <see cref="ObjectResult"/> with status code 400 and content type <c>application/json</c>,
    /// wrapping a <see cref="ResponseDto{String}"/> that includes meta, error code, message and field-level issues.
    /// </returns>
    public static IActionResult Create(ActionContext context)
    {
        var http = context.HttpContext;
        var pdf = http.RequestServices.GetService(typeof(ProblemDetailsFactory)) as ProblemDetailsFactory;
        var vpd = pdf?.CreateValidationProblemDetails(http, context.ModelState);

        // If OpenTelemetry/System.Diagnostics.Activity is active, capture current Trace/Span for correlation.
        var currentActivity = Activity.Current;

        // API version (if any) for this request
        var version = http.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        string versionData = null;
        if (version is not null)
        {
            versionData = $"{version.MajorVersion}.{version.MinorVersion ?? 0}";
        }

        // Flatten ModelState into a list of { field, issue[] } items.
        var errors = ToErrorList(context.ModelState);

        // Build the standardized envelope used across the app.
        var resp = new ResponseDto<string>
        {
            Meta = new MetaDetailDto
            {
                RequestId = http.TraceIdentifier,
                CorrelationId = http.Items[X_CORRELATION_ID].ToString(),
                Version = versionData,
                SpanId = currentActivity?.SpanId.ToString(),
                TraceId = currentActivity?.TraceId.ToString(),
                RequestTimestamp = http.Items[REQUEST_TIMESTAMP] as DateTime?,
                ResponseTimestamp = DateTime.UtcNow
            },
            Error = new ErrorDto
            {
                Code = BAD_REQUEST,
                StatusCode = vpd?.Status ?? StatusCodes.Status400BadRequest,
                Message = vpd?.Title ?? VALIDATION_FAILURES_HAVE_OCCURRED,
                Details = errors
            }
        };

        // Return a 400 response with your unified payload.
        return new ObjectResult(resp)
        {
            StatusCode = vpd?.Status ?? StatusCodes.Status400BadRequest,
            ContentTypes = { TEXT_JSON }
        };
    }

    private static List<ErrorDetailDto> ToErrorList(ModelStateDictionary modelState) =>
        modelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .Select(kv => new ErrorDetailDto
            {
                Field = kv.Key,
                Issue = kv.Value.Errors
                          .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                              ? e.Exception?.Message
                              : e.ErrorMessage)
                          .ToArray()
            })
            .ToList();
}