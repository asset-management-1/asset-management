namespace Be.Haven.ApiCommon.Middlewares;

/// <summary>
/// Converts unhandled request exceptions into the shared API response envelope.
/// </summary>
public sealed class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlerMiddleware"/> class with the specified request delegate,
    /// and logger.
    /// </summary>
    /// <param name="next">The next middleware in the HTTP request pipeline.</param>
    /// <param name="logger">Logger for error handling and diagnostics.</param>
    public ErrorHandlerMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Processes HTTP requests and handles any exceptions that occur during request execution.
    /// Captures and maps exceptions to proper response models before sending the response.
    /// </summary>
    /// <param name="context">The current HTTP context of the request.</param>
    /// <returns>A task representing the asynchronous handling of the HTTP request.</returns>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException exception) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(
                exception,
                REQUEST_CANCELED_BY_CLIENT,
                context.Request.Path,
                context.Request.Method);
        }
        catch (Exception exception)
        {
            // Map the failure once before writing a client-safe response envelope.
            var responseModel = HandleException(context, exception);

            await WriteResponseAsync(context, responseModel);
        }
    }

    /// <summary>
    /// Handles exceptions encountered during the execution of requests, maps them to an appropriate response model,
    /// and constructs error details with relevant metadata.
    /// </summary>
    /// <param name="context">The current HTTP context associated with the ongoing request.</param>
    /// <param name="exception">The exception that occurred during the request pipeline execution.</param>
    /// <returns>A formatted <see cref="ResponseDto{T}"/> containing error details, metadata, and additional trace information.</returns>
    private ResponseDto<string> HandleException(
        HttpContext context,
        Exception exception)
    {
        var currentActivity = Activity.Current;
        var version = context.Features.Get<IApiVersioningFeature>()?.RequestedApiVersion;
        string versionData = null;

        if (version is not null)
        {
            versionData = $"{version.MajorVersion}.{version.MinorVersion ?? 0}";
        }

        var correlationId = context.Items.TryGetValue(X_CORRELATION_ID, out var value)
            ? value?.ToString()
            : null;

        // Preserve request and trace metadata for every mapped exception type.
        var response = new ResponseDto<string>
        {
            Meta = new MetaDetailDto
            {
                CorrelationId = correlationId,
                RequestId = context.TraceIdentifier,
                Version = versionData,
                SpanId = currentActivity?.SpanId.ToString(),
                TraceId = currentActivity?.TraceId.ToString(),
                RequestTimestamp = context.Items[REQUEST_TIMESTAMP] as DateTime?,
                ResponseTimestamp = DateTime.UtcNow
            }
        };

        switch (exception)
        {
            case DistributedLockUnavailableException:
                // Coordination outages fail closed so callers cannot bypass protected workflows.
                response.Error = new ErrorDto
                {
                    Code = SERVICE_UNAVAILABLE,
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Message = SERVICE_UNAVAILABLE_MESSAGE
                };
                _logger.LogError(exception, SERVICE_UNAVAILABLE);
                break;

            case ApiException apiError:
                response.Error = new ErrorDto
                {
                    Code = apiError.ErrorCode ?? BAD_REQUEST,
                    StatusCode = apiError.StatusCode,
                    Message = exception.Message,
                    Details = apiError.Details?.ToList() ?? []
                };
                _logger.LogError(exception, apiError.ErrorCode ?? BAD_REQUEST);
                break;

            case ArgumentException:
                response.Error = new ErrorDto
                {
                    Code = BAD_REQUEST,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exception.Message
                };
                _logger.LogError(exception, BAD_REQUEST);
                break;

            case ValidationException validationError:
                response.Error = new ErrorDto
                {
                    Code = validationError.ErrorCode,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = exception.Message,
                    Details = validationError.Errors
                };
                _logger.LogError(exception, BAD_REQUEST);
                break;

            case KeyNotFoundException:
                response.Error = new ErrorDto
                {
                    Code = NOT_FOUND,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = exception.Message
                };
                _logger.LogError(exception, NOT_FOUND);
                break;

            case UnauthorizedAccessException:
            case AuthenticationFailureException:
                response.Error = new ErrorDto
                {
                    Code = UNAUTHORIZED,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = exception.Message
                };
                _logger.LogError(exception, UNAUTHORIZED);
                break;

            case HttpStatusCodeException httpStatusError:
                var statusCode = httpStatusError.StatusCode;

                var errorCode = !string.IsNullOrEmpty(httpStatusError.ErrorCode)
                    ? httpStatusError.ErrorCode
                    : statusCode switch
                    {
                        StatusCodes.Status400BadRequest => BAD_REQUEST,
                        StatusCodes.Status401Unauthorized => UNAUTHORIZED,
                        StatusCodes.Status403Forbidden => FORBIDDEN,
                        StatusCodes.Status404NotFound => NOT_FOUND,
                        StatusCodes.Status405MethodNotAllowed => METHOD_NOT_ALLOWED,
                        StatusCodes.Status406NotAcceptable => NOT_ACCEPTABLE,
                        StatusCodes.Status413PayloadTooLarge => BAD_REQUEST,
                        StatusCodes.Status415UnsupportedMediaType => UNSUPPORTED_MEDIA_TYPE,
                        StatusCodes.Status429TooManyRequests => MANY_REQUESTS,
                        StatusCodes.Status500InternalServerError => INTERNAL_SERVER,
                        StatusCodes.Status503ServiceUnavailable => SERVICE_UNAVAILABLE,
                        _ => INTERNAL_SERVER
                    };

                response.Error = new ErrorDto
                {
                    Code = errorCode,
                    StatusCode = statusCode,
                    Message = exception.Message
                };

                _logger.LogError(exception, errorCode);
                break;

            default:
                // Unexpected exceptions are logged with details but exposed with a generic client-safe message.
                response.Error = new ErrorDto
                {
                    Code = INTERNAL_SERVER,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = UNEXPECTED_SERVER_ERROR
                };
                _logger.LogError(exception, INTERNAL_SERVER);
                break;
        }

        return response;
    }

    /// <summary>
    /// Writes the mapped error response when the client connection remains available.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="response">The mapped response envelope.</param>
    /// <returns>A task representing the response write.</returns>
    private async Task WriteResponseAsync(
        HttpContext context,
        ResponseDto<string> response)
    {
        try
        {
            if (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogWarning(
                    SKIPPED_WRITING_ERROR_RESPONSE_BECAUSE_REQUEST_ABORTED,
                    context.Request.Path,
                    context.Request.Method);
                return;
            }

            if (context.Response.HasStarted)
            {
                _logger.LogWarning(RESPONSE_ALREADY_STARTED);
                return;
            }

            // Write only after cancellation and response-start guards have passed.
            context.Response.StatusCode = response.Error.StatusCode;
            context.Response.ContentType = TEXT_JSON;

            await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
        }
        catch (OperationCanceledException exception) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(
                exception,
                REQUEST_CANCELED_WHILE_WRITING_ERROR_RESPONSE,
                context.Request.Path,
                context.Request.Method);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, RESPONSE_CREATION_ERROR);
        }
    }
}
