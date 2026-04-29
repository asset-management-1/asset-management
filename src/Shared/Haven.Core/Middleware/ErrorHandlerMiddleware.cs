namespace Haven.Core.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlerMiddleware"/> class with the specified request delegate,
    /// service scope factory, and logger.
    /// </summary>
    /// <param name="next">The next middleware in the HTTP request pipeline.</param>
    /// <param name="scopeFactory">Factory for creating service scopes.</param>
    /// <param name="logger">Logger for error handling and diagnostics.</param>
    public ErrorHandlerMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
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
        catch (OperationCanceledException ex) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                REQUEST_CANCELED_BY_CLIENT,
                context.Request.Path,
                context.Request.Method);
        }
        catch (Exception error)
        {
            var responseModel = HandleException(context, error);

            await CreateAndWriteResponseAsync(context, responseModel);
        }
    }

    /// <summary>
    /// Handles exceptions encountered during the execution of requests, maps them to an appropriate response model,
    /// and constructs error details with relevant metadata.
    /// </summary>
    /// <param name="context">The current HTTP context associated with the ongoing request.</param>
    /// <param name="error">The exception that occurred during the request pipeline execution.</param>
    /// <returns>A formatted <see cref="ResponseDto{T}"/> containing error details, metadata, and additional trace information.</returns>
    private ResponseDto<string> HandleException(
        HttpContext context,
        Exception error)
    {
        // Initialize a response model
        var currentActivity = Activity.Current;
        
        // API version (if any) for this request
        var version = context.GetRequestedApiVersion();
        string versionData = null;
        if (version is not null)
        {
            versionData = $"{version.MajorVersion}.{version.MinorVersion ?? 0}";
        }

        var response = new ResponseDto<string>
        {
            Meta = new MetaDetailDto
            {
                CorrelationId = context.Items[X_CORRELATION_ID].ToString(),
                RequestId = context.TraceIdentifier,
                Version = versionData,
                SpanId = currentActivity?.SpanId.ToString(),
                TraceId = currentActivity?.TraceId.ToString(),
                RequestTimestamp = context.Items[REQUEST_TIMESTAMP] as DateTime?,
                ResponseTimestamp = DateTime.UtcNow
            }
        };
        // Map exception types to appropriate HTTP status codes and messages
        switch (error)
        {
            case ApiException apiError:
                response.Error = new ErrorDto
                {
                    Code = apiError.ErrorCode ?? BAD_REQUEST,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = error.Message
                };
                _logger.LogError(error, apiError.ErrorCode ?? BAD_REQUEST);
                break;

            case ArgumentException:
                response.Error = new ErrorDto
                {
                    Code = BAD_REQUEST,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = error.Message
                };
                _logger.LogError(error, BAD_REQUEST);
                break;

            case ValidationException validationError:
                response.Error = new ErrorDto
                {
                    Code = validationError.ErrorCode,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = error.Message,
                    Details = validationError.Errors
                };
                _logger.LogError(error, BAD_REQUEST);
                break;

            case KeyNotFoundException:
                response.Error = new ErrorDto
                {
                    Code = NOT_FOUND,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = error.Message
                };
                _logger.LogError(error, NOT_FOUND);
                break;

            case UnauthorizedAccessException:

            case AuthenticationFailureException:
                response.Error = new ErrorDto
                {
                    Code = UNAUTHORIZED,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = error.Message
                };
                _logger.LogError(error, UNAUTHORIZED);
                break;

            case HttpStatusCodeException httpStatusError:
                var statusCode = httpStatusError.StatusCode;

                var errorCode = !string.IsNullOrEmpty(httpStatusError.ErrorCode) ? httpStatusError.ErrorCode : statusCode switch
                {
                    StatusCodes.Status400BadRequest   => BAD_REQUEST,
                    StatusCodes.Status401Unauthorized => UNAUTHORIZED,
                    StatusCodes.Status404NotFound     => NOT_FOUND,
                    StatusCodes.Status503ServiceUnavailable     => SERVICE_UNAVAILABLE,
                    _                                 => INTERNAL_SERVER
                };

                response.Error = new ErrorDto
                {
                    Code = errorCode,
                    StatusCode = statusCode,
                    Message = error.Message
                };

                _logger.LogError(error, errorCode);
                break;

            default:
                response.Error = new ErrorDto
                {
                    Code = INTERNAL_SERVER,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = error.Message
                };
                _logger.LogError(error, INTERNAL_SERVER);
                break;
        }

        return response;
    }

    /// <summary>
    /// Helper method to create and write the response asynchronously.
    /// </summary>
    /// <param name="context"> The function context. </param>
    /// <param name="response"> The response model to write. </param>
    /// <returns> A task representing the asynchronous operation. </returns>
    private async Task CreateAndWriteResponseAsync(
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
            using var scope = _scopeFactory.CreateScope();
            context.Response.StatusCode = response.Error.StatusCode;
            context.Response.ContentType = TEXT_JSON;

            await context.Response.WriteAsJsonAsync(response, context.RequestAborted);
        }
        catch (OperationCanceledException ex) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                REQUEST_CANCELED_WHILE_WRITING_ERROR_RESPONSE,
                context.Request.Path,
                context.Request.Method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, RESPONSE_CREATION_ERROR);
        }
    }
}