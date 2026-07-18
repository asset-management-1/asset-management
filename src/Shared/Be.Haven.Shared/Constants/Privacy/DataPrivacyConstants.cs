namespace Be.Haven.Shared.Constants.Privacy;

/// <summary>
/// Provides constants related to data privacy, including default masks and sensitive key identifiers.
/// </summary>
public static class DataPrivacyConstants
{
    /// <summary>
    /// The default string mask used to replace or obscure sensitive data.
    /// </summary>
    public const string DEFAULT_MASK = "******";

    /// <summary>
    /// A collection of case-insensitive string keys deemed sensitive for data privacy purposes.
    /// </summary>
    public static readonly HashSet<string> SENSITIVE_KEYS = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "pwd", "pass",
        "newPassword", "confirmPassword",
        "secret", "clientSecret", "secretKey",
        "token", "accessToken", "access_token",
        "refreshToken", "refresh_token",
        "externalToken", "external_token",
        "idToken", "id_token",
        "authorization", "apiKey", "api_key",
        "otp", "identifierValue", "nationalId", "cccd"
    };

    /// <summary>
    /// A regular expression used to identify and match sensitive plain text patterns,
    /// such as passwords or secrets, in inputs for masking purposes.
    /// </summary>
    public static readonly Regex PLAIN_TEXT_REGEX = new(
        @"(?ix)
        \b
        (password|pwd|pass|newpassword|confirmpassword
        |secret|clientsecret|secretkey
        |token|accesstoken|access_token
        |refreshtoken|refresh_token
        |externaltoken|external_token
        |idtoken|id_token|authorization
        |apikey|api_key|otp|identifiervalue|nationalid|cccd)
        \b
        (\s*[:=]\s*)
        ([""']?)
        (.*?)
        \3
        (?=,|\s|\||;|}|$)",
        RegexOptions.Compiled);
}
