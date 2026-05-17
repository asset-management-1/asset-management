namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides safe helpers for reading third-party HTTP response content for logs.
/// </summary>
public static class HttpResponseContentHelper
{
    /// <summary>
    /// Reads and masks the response content from an HTTP content instance.
    /// </summary>
    /// <param name="content">The HTTP content that may contain provider error details.</param>
    /// <param name="cancellationToken">The token used to cancel the content read.</param>
    /// <returns>The masked response content, or fallback text when content is unavailable.</returns>
    public static async Task<string> ReadMaskedContentAsync(
        HttpContent content,
        CancellationToken cancellationToken = default)
    {
        // Missing provider content is logged as a stable fallback instead of producing blank log fields.
        if (content is null)
        {
            return DEFAULT_TEXT;
        }

        try
        {
            // Read the provider body once and mask any sensitive fields before it reaches structured logs.
            var responseContent = await content.ReadAsStringAsync(cancellationToken);

            return string.IsNullOrWhiteSpace(responseContent)
                ? DEFAULT_TEXT
                : LogMaskingHelper.MaskAllSensitiveData(responseContent);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Response-body logging is diagnostic only; unreadable content should not hide the original failure.
            return DEFAULT_TEXT;
        }
    }
}
