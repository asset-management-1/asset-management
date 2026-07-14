namespace Be.Haven.Tests.Common;

/// <summary>
/// Keeps GCP integration-bound unit tests runnable on machines without Application Default Credentials.
/// </summary>
public static class GcpCredentialTestGuard
{
    private const string DEFAULT_CREDENTIALS_MISSING_TEXT = "default credentials were not found";
    private const string DEFAULT_CREDENTIALS_SKIP_REASON =
        "Skipped because Google Application Default Credentials are unavailable on this machine.";

    /// <summary>
    /// Runs one GCP client assertion and skips only when the Google SDK reports missing default credentials.
    /// </summary>
    /// <param name="assertion">The GCP client construction or resolution assertion.</param>
    public static void ExecuteOrSkip(Action assertion)
    {
        try
        {
            assertion();
        }
        catch (InvalidOperationException exception)
            when (exception.Message.Contains(DEFAULT_CREDENTIALS_MISSING_TEXT, StringComparison.OrdinalIgnoreCase))
        {
            throw Xunit.Sdk.SkipException.ForSkip(DEFAULT_CREDENTIALS_SKIP_REASON);
        }
    }

    /// <summary>
    /// Runs one asynchronous GCP client assertion and skips only when the Google SDK reports missing default credentials.
    /// </summary>
    /// <param name="assertion">The asynchronous GCP client construction or resolution assertion.</param>
    /// <returns>A task that completes when the assertion passes or is skipped for unavailable local credentials.</returns>
    public static async Task ExecuteAsyncOrSkip(Func<Task> assertion)
    {
        try
        {
            await assertion();
        }
        catch (InvalidOperationException exception)
            when (exception.Message.Contains(DEFAULT_CREDENTIALS_MISSING_TEXT, StringComparison.OrdinalIgnoreCase))
        {
            throw Xunit.Sdk.SkipException.ForSkip(DEFAULT_CREDENTIALS_SKIP_REASON);
        }
    }
}
