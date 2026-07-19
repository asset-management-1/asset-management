namespace Authentication.Api.Constants;

/// <summary>
/// Centralised API error messages used for validation and authorisation responses.
/// These constants represent user-facing messages that describe why a request
/// was rejected due to missing or invalid data.
/// </summary>
public static class ApiErrorConstants
{
    /// <summary>
    /// Startup validation message for incomplete Google or Facebook provider configuration.
    /// </summary>
    public const string EXTERNAL_PROVIDER_OPTIONS_INVALID = "Google and Facebook external authentication configuration is incomplete.";
}
