namespace Be.Haven.Core.Helpers;

/// <summary>
/// Creates common operation-status responses.
/// </summary>
public static class OperationStatusResponseHelper
{
    /// <summary>
    /// Creates a successful operation-status response.
    /// </summary>
    /// <param name="message">The success message returned to the caller.</param>
    /// <returns>The successful operation-status response.</returns>
    public static OperationStatusResponseDto Success(string message)
    {
        // Keep simple success payload creation consistent across services.
        return new OperationStatusResponseDto
        {
            IsSuccess = true,
            Message = message
        };
    }
}
