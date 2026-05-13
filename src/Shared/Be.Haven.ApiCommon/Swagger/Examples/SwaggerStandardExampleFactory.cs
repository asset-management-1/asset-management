namespace Be.Haven.ApiCommon.Swagger.Examples;

/// <summary>
/// Builds shared response-envelope examples for Swagger documentation.
/// </summary>
public static class SwaggerStandardExampleFactory
{
    /// <summary>
    /// Builds a successful response envelope for a typed payload.
    /// </summary>
    /// <typeparam name="TData">The response payload type.</typeparam>
    /// <param name="data">The sample response payload.</param>
    /// <returns>The standardized success response example.</returns>
    public static ResponseDto<TData> BuildSuccessEnvelope<TData>(TData data)
    {
        // Swagger examples use fake tracing metadata so consumers see the full response envelope.
        return new ResponseDto<TData>(data)
        {
            Meta = BuildMeta()
        };
    }

    /// <summary>
    /// Builds a standard error response envelope for one HTTP status code.
    /// </summary>
    /// <param name="statusCode">The documented HTTP status code.</param>
    /// <returns>The standardized error response example.</returns>
    public static ResponseDto<object> Error(int statusCode)
    {
        // Status-specific examples keep the same envelope while showing the expected code/message contract.
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => ValidationError(),
            StatusCodes.Status401Unauthorized => BuildError(UNAUTHORIZED, INVALID_TOKEN, statusCode),
            StatusCodes.Status403Forbidden => BuildError(FORBIDDEN, MISSING_PERMISSION, statusCode),
            StatusCodes.Status404NotFound => BuildError(NOT_FOUND, MSG_NOT_FOUND, statusCode),
            StatusCodes.Status405MethodNotAllowed => BuildError(METHOD_NOT_ALLOWED, MSG_METHOD_NOT_ALLOWED, statusCode),
            StatusCodes.Status406NotAcceptable => BuildError(NOT_ACCEPTABLE, MSG_NOT_ACCEPTABLE, statusCode),
            StatusCodes.Status413PayloadTooLarge => BuildError(BAD_REQUEST, MSG_PAYLOAD_TOO_LARGE, statusCode),
            StatusCodes.Status415UnsupportedMediaType => BuildError(UNSUPPORTED_MEDIA_TYPE, MSG_UNSUPPORTED_MEDIA_TYPE, statusCode),
            StatusCodes.Status429TooManyRequests => BuildError(MANY_REQUESTS, RedisConstants.ErrorMessage.ACCOUNT_LOCKED, statusCode),
            StatusCodes.Status503ServiceUnavailable => BuildError(SERVICE_UNAVAILABLE, SwaggerExampleConstants.SERVICE_UNAVAILABLE_MESSAGE, statusCode),
            _ => BuildError(INTERNAL_SERVER, UNEXPECTED_SERVER_ERROR, StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Builds a validation error response envelope.
    /// </summary>
    /// <returns>The standardized validation error response example.</returns>
    public static ResponseDto<object> ValidationError()
    {
        // Validation details intentionally model Issue as an array because the runtime groups messages per field.
        return new ResponseDto<object>(
            VALIDATION_ERROR,
            VALIDATION_FAILURES_HAVE_OCCURRED,
            StatusCodes.Status400BadRequest,
            [
                new ErrorDetailDto
                {
                    Field = SwaggerExampleConstants.VALIDATION_FIELD_EMAIL,
                    Issue = new[] { SwaggerExampleConstants.VALIDATION_EMAIL_ISSUE }
                },
                new ErrorDetailDto
                {
                    Field = SwaggerExampleConstants.VALIDATION_FIELD_PASSWORD,
                    Issue = new[] { SwaggerExampleConstants.VALIDATION_PASSWORD_ISSUE }
                }
            ],
            BuildMeta());
    }

    /// <summary>
    /// Builds a response metadata example.
    /// </summary>
    /// <returns>The standardized response metadata example.</returns>
    public static MetaDetailDto BuildMeta()
    {
        // Fixed values make examples stable across Swagger renders.
        return new MetaDetailDto
        {
            RequestId = SwaggerExampleConstants.EXAMPLE_REQUEST_ID,
            CorrelationId = SwaggerExampleConstants.EXAMPLE_CORRELATION_ID,
            TraceId = SwaggerExampleConstants.EXAMPLE_TRACE_ID,
            SpanId = SwaggerExampleConstants.EXAMPLE_SPAN_ID,
            Version = SwaggerExampleConstants.EXAMPLE_VERSION,
            RequestTimestamp = new DateTime(2026, 5, 11, 3, 0, 0, DateTimeKind.Utc),
            ResponseTimestamp = new DateTime(2026, 5, 11, 3, 0, 1, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Builds a non-validation error response envelope.
    /// </summary>
    /// <param name="code">The public error code.</param>
    /// <param name="message">The public error message.</param>
    /// <param name="statusCode">The HTTP status code associated with the error.</param>
    /// <returns>The standardized error response example.</returns>
    private static ResponseDto<object> BuildError(string code, string message, int statusCode)
    {
        // Error examples keep Data empty because runtime errors are always returned in Error.
        return new ResponseDto<object>(
            code,
            message,
            statusCode,
            details: [],
            BuildMeta());
    }
}
