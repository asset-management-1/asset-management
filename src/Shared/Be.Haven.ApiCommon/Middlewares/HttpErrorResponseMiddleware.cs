namespace Be.Haven.ApiCommon.Middlewares;

/// <summary>
/// Formats selected pipeline HTTP error status codes (404/405/406/413/415/429)
/// into the application's standard <see cref="ResponseDto{T}"/> error payload.
/// - 404 Not Found (route not matched / version mismatch)
/// - 405 Method Not Allowed
/// - 406 Not Acceptable
/// - 413 Payload Too Large
/// - 415 Unsupported Media Type
/// - 429 Too Many Requests
/// </summary>
public sealed class HttpErrorResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpErrorResponseMiddleware> _logger;

    // Cached supported API major versions (v1, v2, ...) to avoid recomputing per request.
    private readonly HashSet<int> _supportedMajors;

    /// <summary>
    /// Initializes the middleware and caches supported API versions once at startup.
    /// </summary>
    public HttpErrorResponseMiddleware(
        RequestDelegate next,
        ILogger<HttpErrorResponseMiddleware> logger,
        IApiVersionDescriptionProvider versionProvider)
    {
        _next = next;
        _logger = logger;

        _supportedMajors = versionProvider.ApiVersionDescriptions
                                          .Select(d => d.ApiVersion.MajorVersion ?? 0)
                                          .Distinct()
                                          .ToHashSet();
    }

    /// <summary>
    /// Intercepts the HTTP request to evaluate the status code of the response and wraps selected framework/pipeline
    /// HTTP status codes into a standardized response structure if necessary.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request and response.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation that processes the HTTP request pipeline and
    /// wraps specific HTTP status codes in the response if applicable.
    /// </returns>
    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (!ShouldFormatStatusResponse(context, out var path, out var status))
            return;

        // If the endpoint matched and the controller returned NotFound() intentionally, do not override the controller's response.
        if (status == StatusCodes.Status404NotFound && context.GetEndpoint() != null)
            return;

        var (errorCode, message) = MapStatus(status, path);

        _logger.LogDebug(
            ApiVersionLogs.LOG_HTTP_ERROR_GENERATED,
            status,
            errorCode,
            path);

        context.Response.ContentType = TEXT_JSON;
        await context.Response.WriteAsJsonAsync(
            BuildResponse(context, status, errorCode, message),
            context.RequestAborted);
    }

    /// <summary>
    /// Determines whether the response should be wrapped.
    /// We only wrap:
    /// - Responses with specific status codes
    /// - Responses that are still empty (no body written)
    /// - Responses that are not Swagger UI requests
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="path">The relative request path associated with the HTTP context.</param>
    /// <param name="status">The HTTP response status code.</param>
    /// <returns>
    /// True if the response should be wrapped; otherwise, false.
    /// </returns>
    private static bool ShouldFormatStatusResponse(HttpContext context, out string path, out int status)
    {
        path = context.Request.Path.Value ?? string.Empty;
        status = context.Response.StatusCode;

        // Skip if headers/body already started, or if there's already a response body.
        if (context.Response.HasStarted || context.Response.ContentLength is > 0)
            return false;

        // Skip Swagger UI endpoints (otherwise Swagger UI may display errors).
        if (path.StartsWith(SWAGGER_PATH_PREFIX, StringComparison.OrdinalIgnoreCase))
            return false;

        return status is
            StatusCodes.Status404NotFound or
            StatusCodes.Status405MethodNotAllowed or
            StatusCodes.Status406NotAcceptable or
            StatusCodes.Status413PayloadTooLarge or
            StatusCodes.Status415UnsupportedMediaType or
            StatusCodes.Status429TooManyRequests;
    }

    /// <summary>
    /// Maps an HTTP status code to a corresponding application-defined error code and message.
    /// </summary>
    /// <param name="status">The HTTP status code to be mapped.</param>
    /// <param name="path">The request path that triggered the status code.</param>
    /// <returns>
    /// A tuple containing:
    /// - Code: The application-defined error code corresponding to the status.
    /// - Message: The error message that provides additional details about the status.
    /// </returns>
    private (string Code, string Message) MapStatus(int status, string path)
    {
        return status switch
        {
            StatusCodes.Status404NotFound => MapNotFound(path),

            StatusCodes.Status405MethodNotAllowed =>
                (METHOD_NOT_ALLOWED,
                    MSG_METHOD_NOT_ALLOWED),

            StatusCodes.Status406NotAcceptable =>
                (NOT_ACCEPTABLE,
                    MSG_NOT_ACCEPTABLE),

            StatusCodes.Status415UnsupportedMediaType =>
                (UNSUPPORTED_MEDIA_TYPE,
                    MSG_UNSUPPORTED_MEDIA_TYPE),

            StatusCodes.Status413PayloadTooLarge =>
                (BAD_REQUEST,
                    MSG_PAYLOAD_TOO_LARGE),

            StatusCodes.Status429TooManyRequests =>
                (MANY_REQUESTS,
                    MSG_TOO_MANY_REQUESTS),

            _ =>
                (INTERNAL_SERVER,
                    MSG_REQUEST_FAILED)
        };
    }

    /// <summary>
    /// Handles 404 responses:
    /// - If the URL contains an unsupported API version segment (v{n}), return API_VERSION_NOT_SUPPORTED.
    /// - Otherwise return the generic NOT_FOUND code/message.
    /// </summary>
    /// <param name="path">The incoming request path, which may contain the API version segment.</param>
    /// <returns>A tuple containing a unique code and a descriptive message for the 404 Not Found status.</returns>
    private (string Code, string Message) MapNotFound(string path)
    {
        var major = GetApiMajorVersionFromPath(path);
        var isVersionMismatch = major is not null && !_supportedMajors.Contains(major.Value);

        if (isVersionMismatch)
        {
            _logger.LogInformation(
                ApiVersionLogs.LOG_VERSION_MISMATCH,
                major,
                path);

            return (API_VERSION_NOT_SUPPORTED,
                string.Format(MSG_VERSION_NOT_SUPPORTED_FORMAT, path));
        }

        return (NOT_FOUND,
            MSG_NOT_FOUND);
    }

    /// <summary>
    /// Finds the API major version from the last version token in the URL path.
    /// A version token is a segment like "v1" or "V12" (exactly 'v' + digits).
    /// </summary>
    /// <param name="requestPath">
    /// The API request path from which to extract the major version number.
    /// </param>
    /// <returns>
    /// The major version number as an integer, if successfully extracted; otherwise, null if the version is not found or valid in the path.
    /// </returns>
    private static int? GetApiMajorVersionFromPath(string requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
            return null;

        var pathSegments = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Iterate from the end to get the token closest to the API route/controller.
        for (var index = pathSegments.Length - 1; index >= 0; index--)
        {
            if (TryParseApiVersionSegment(pathSegments[index], out var majorVersion))
                return majorVersion;
        }

        return null;
    }

    /// <summary>
    /// Parses a single path segment as a version token: "v{digits}" (case-insensitive).
    /// Examples: "v1" => 1, "V12" => 12.
    /// </summary>
    /// <param name="segment">
    /// The string segment of the path to evaluate for an API version identifier.
    /// </param>
    /// <param name="majorVersion">
    /// When this method returns, contains the extracted major version number if the parsing succeeds; otherwise, the value is 0.
    /// </param>
    /// <returns>
    /// Returns <c>true</c> if the segment successfully represents a valid API version identifier; otherwise, <c>false</c>.
    /// </returns>
    private static bool TryParseApiVersionSegment(string segment, out int majorVersion)
    {
        majorVersion = 0;

        // Must be at least: 'v' + 1 digit
        if (string.IsNullOrEmpty(segment) || segment.Length < 2)
            return false;

        var prefix = segment[0];
        if (prefix != VERSION_CHAR_LOWER && prefix != VERSION_CHAR_UPPER)
            return false;

        // Everything after 'v' must be digits only
        var numericPart = segment.AsSpan(1);
        foreach (var t in numericPart)
        {
            if (!char.IsDigit(t))
                return false;
        }

        return int.TryParse(numericPart, out majorVersion);
    }

    /// <summary>
    /// Builds the standardized ResponseDto envelope with tracing/correlation metadata.
    /// </summary>
    /// <param name="context">The current HTTP context providing request and response information.</param>
    /// <param name="status">The HTTP status code representing the outcome of the request.</param>
    /// <param name="code">A string code that represents a more specific error or status condition.</param>
    /// <param name="message">A detailed message describing the status or error condition.</param>
    /// <returns>A constructed <see cref="ResponseDto{T}"/> containing error details and metadata.</returns>
    private static ResponseDto<string> BuildResponse(HttpContext context, int status, string code, string message)
    {
        var activity = Activity.Current;
        
        // API version (if any) for this request
        var version = context.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        string versionData = null;
        if (version is not null)
        {
            versionData = $"{version.MajorVersion}.{version.MinorVersion ?? 0}";
        }
        
        return new ResponseDto<string>
        {
            Error = new ErrorDto
            {
                Code = code,
                StatusCode = status,
                Message = message
            },
            Meta = new MetaDetailDto
            {
                CorrelationId = context.Items[X_CORRELATION_ID]?.ToString(),
                RequestId = context.TraceIdentifier,
                Version = versionData,
                SpanId = activity?.SpanId.ToString(),
                TraceId = activity?.TraceId.ToString(),
                RequestTimestamp = context.Items[REQUEST_TIMESTAMP] as DateTime?,
                ResponseTimestamp = DateTime.UtcNow
            }
        };
    }
}