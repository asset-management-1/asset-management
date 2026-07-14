namespace Be.Haven.Core.Exceptions;

/// <summary>
/// Represents a client-safe API error with an optional application error code.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Gets the custom error code associated with this exception.
    /// This value is typically used by the middleware to return
    /// a meaningful error response to the client.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the HTTP status code that should be used for the API error response.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets optional public-safe business details for contract-approved cases.
    /// </summary>
    public IReadOnlyList<ErrorDetailDto> Details { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    public ApiException() : base()
    {
        // Application exceptions default to Bad Request unless a caller provides a more specific status.
        StatusCode = StatusCodes.Status400BadRequest;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class
    /// with a descriptive error message.
    /// </summary>
    /// <param name="message">The error message describing the issue.</param>
    public ApiException(string message) : base(message)
    {
        // Application exceptions default to Bad Request unless a caller provides a more specific status.
        StatusCode = StatusCodes.Status400BadRequest;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class
    /// with a descriptive error message and a specific application error code.
    /// </summary>
    /// <param name="message">The error message describing the issue.</param>
    /// <param name="errorCode">
    /// A custom application error code used to identify this error type.
    /// </param>
    public ApiException(string message, string errorCode)
        : base(message)
    {
        // Keep existing callers as business errors while allowing newer callers to opt into other statuses.
        ErrorCode = errorCode;
        StatusCode = StatusCodes.Status400BadRequest;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class with a message, error code, and HTTP status code.
    /// </summary>
    /// <param name="message">The error message describing the issue.</param>
    /// <param name="errorCode">The application error code used to identify this error type.</param>
    /// <param name="statusCode">The HTTP status code that should be returned to the caller.</param>
    public ApiException(string message, string errorCode, int statusCode)
        : base(message)
    {
        // Preserve the business error code while carrying the HTTP status for the response middleware.
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class with a message, error code, status, and public-safe details.
    /// </summary>
    /// <param name="message">The error message describing the issue.</param>
    /// <param name="errorCode">The application error code used to identify this error type.</param>
    /// <param name="statusCode">The HTTP status code that should be returned to the caller.</param>
    /// <param name="details">Public-safe structured details for a contract-approved response payload.</param>
    public ApiException(
        string message,
        string errorCode,
        int statusCode,
        IReadOnlyList<ErrorDetailDto> details)
        : base(message)
    {
        // Details are reserved for public-safe contracts, not diagnostic or internal guard data.
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details ?? [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class
    /// with a formatted message string and dynamic message arguments.
    /// </summary>
    /// <param name="message">The message format string.</param>
    /// <param name="args">Values to format into the message string.</param>
    public ApiException(string message, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, message, args))
    {
        // Formatted application errors keep the legacy Bad Request behavior.
        StatusCode = StatusCodes.Status400BadRequest;
    }
}
