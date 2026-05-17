namespace Be.Haven.Core.Exceptions;


/// <summary>
/// Exception thrown when an HTTP request results in an error status code.
/// </summary>
public class HttpStatusCodeException : Exception
{
    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the error code that provides additional details about the exception.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpStatusCodeException"/> class with a specified error message and status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="statusCode">The HTTP status code that caused the exception.</param>
    public HttpStatusCodeException(
        string message,
        int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Exception thrown when an HTTP request results in an error status code.
    /// </summary>
    public HttpStatusCodeException(
        string message,
        string errorCode,
        int statusCode) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
