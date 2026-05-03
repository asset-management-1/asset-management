namespace Be.Haven.ApiCommon.Filters;

public sealed class LogContextFilter : IAsyncActionFilter, IAsyncResultFilter, IOrderedFilter
{
    public int Order => int.MaxValue;

    private readonly ILogger<LogContextFilter> _logger;
    
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };
    
    public LogContextFilter(ILogger<LogContextFilter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executed before the action method is invoked. This is used to capture and log the
    /// sanitized request arguments (after model binding).
    /// </summary>
    /// <param name="context">
    /// The current <see cref="ActionExecutingContext"/>, which contains action arguments,
    /// HttpContext, and action metadata.
    /// </param>
    /// <param name="next">
    /// A delegate that, when invoked, will execute the next filter in the pipeline
    /// or the action itself.
    /// </param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Collect all action arguments after model binding (JSON, form, query, route, etc.)
        // and sanitize them (e.g., strip IFormFile content, keep only metadata).
        var requestObject = context.ActionArguments.SanitizeArgs();
    
        // Serialize the sanitized arguments:
        //  - null   → null
        //  - string → keep as-is
        //  - other  → convert to JSON, mask sensitive fields, and return compact JSON
        var requestBodyJson = requestObject switch
        {
            null => null,
            string s => s,
            _ => JsonConvert.SerializeObject(requestObject, _jsonSerializerSettings)
        };

        // Store the serialized request body in HttpContext so it can be reused
        // by the result filter or any logging middleware.
        context.HttpContext.Items[REQUEST_BODY] = requestBodyJson;

        // Log the incoming HTTP request with the resolved action name and arguments.
        _logger.LogInformation(
            LOG_MSG_START,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            context.ActionDescriptor.DisplayName,
            requestBodyJson
        );

        // Continue to the next filter / controller action.
        await next();
    }

    /// <summary>
    /// Executes after the action result has been produced, allowing the response payload
    /// to be captured and attached to the HTTP context for later logging or processing.
    /// </summary>
    /// <param name="context">
    /// The result execution context, containing the HTTP context, action result,
    /// and metadata about the current request/response.
    /// </param>
    /// <param name="next">
    /// A delegate that represents the next result filter or the result itself.
    /// Invoking this delegate continues the execution pipeline.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Serialize the response payload (only handle ObjectResult: Ok(object), BadRequest(object), etc.).
        string responseBodyJson = null;
        if (context.Result is ObjectResult { Value: not null } objectResult)
        {
            responseBodyJson = JsonConvert.SerializeObject(objectResult.Value, _jsonSerializerSettings);
        }

        // Retrieve the serialized request body captured in OnActionExecutionAsync.
        var requestBodyJson = context.HttpContext.Items[REQUEST_BODY] as string;

        // Log the completed HTTP request with status code, request body, and response body.
        _logger.LogInformation(
            LOG_MSG_FINISH,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            context.HttpContext.Response.StatusCode,
            requestBodyJson,
            responseBodyJson
        );

        // Continue the pipeline and write the response to the client.
        await next();
    }
}