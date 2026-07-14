namespace Authentication.Api.Filters;

/// <summary>
/// Requires client device metadata headers before an endpoint issues or refreshes authentication tokens.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireClientDeviceInfoAttribute : Attribute, IAsyncResourceFilter
{
    private static readonly string[] RequiredHeaders =
    [
        ClientDeviceHeaders.DEVICE_ID,
        ClientDeviceHeaders.DEVICE_NAME,
        ClientDeviceHeaders.DEVICE_TYPE,
        ClientDeviceHeaders.USER_AGENT
    ];

    /// <summary>
    /// Validates required client device headers before token issuance reaches model binding, handlers, and services.
    /// </summary>
    /// <param name="context">The current MVC resource context.</param>
    /// <param name="next">The delegate that continues resource execution.</param>
    /// <returns>A task that completes when validation and action execution finish.</returns>
    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        // Token issuance needs the full metadata bundle before services read the normalized accessor.
        if (HasRequiredDeviceInfo(context.HttpContext.Request.Headers))
        {
            await next();
            return;
        }

        // Keep the client error generic so missing metadata details do not leak into service code paths.
        throw new ApiException(
            ApplicationErrorConstants.AccountErrors.DEVICE_INFO_REQUIRED_MESSAGE,
            ApplicationErrorConstants.TokenErrorCodes.AUTH_DEVICE_INFO_REQUIRED,
            StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Checks that all required client device headers are present and non-blank.
    /// </summary>
    /// <param name="headers">The HTTP request headers.</param>
    /// <returns><c>true</c> when all required metadata is available; otherwise <c>false</c>.</returns>
    private static bool HasRequiredDeviceInfo(IHeaderDictionary headers)
    {
        // Every required header must be present and meaningful before token metadata is captured.
        return RequiredHeaders.All(headerName =>
            headers.TryGetValue(headerName, out var value)
            && !string.IsNullOrWhiteSpace(value.ToString()));
    }
}
