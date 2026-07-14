namespace Authentication.Api.Constants;

/// <summary>
/// Contains constant string values representing Authentication API route segments.
/// </summary>
public static class ApiRouteConstants
{
    /// <summary>
    /// Route segment for the username-and-password login endpoint.
    /// </summary>
    public const string LOGIN = "login";

    /// <summary>
    /// Route segment for the logout endpoint.
    /// </summary>
    public const string LOGOUT = "logout";

    /// <summary>
    /// Route segment for the refresh-token endpoint.
    /// </summary>
    public const string REFRESH_TOKEN = "refresh-token";

    /// <summary>
    /// Route segment for the external-login endpoint.
    /// </summary>
    public const string EXTERNAL_LOGIN = "external-login";

    /// <summary>
    /// Route segment for the external-provider link endpoint.
    /// </summary>
    public const string EXTERNAL_LINK = "external-link";

    /// <summary>
    /// Route segment for the external-provider unlink endpoint.
    /// </summary>
    public const string EXTERNAL_UNLINK = "external-unlink";

    /// <summary>
    /// Route segment for the forgot-password OTP request endpoint.
    /// </summary>
    public const string FORGOT_PASSWORD = "forgot-password";

    /// <summary>
    /// Route segment for OTP-verification endpoints.
    /// </summary>
    public const string VERIFY_OTP = "verify-otp";

    /// <summary>
    /// Route segment for password-change endpoints.
    /// </summary>
    public const string CHANGE_PASSWORD = "change-password";

    /// <summary>
    /// Route segment for the current-user info endpoint.
    /// </summary>
    public const string USER_INFO = "user-info";

    /// <summary>
    /// Route segment for the change-email OTP request endpoint.
    /// </summary>
    public const string CHANGE_EMAIL = "change-email";

    /// <summary>
    /// Route segment for the change-email OTP verification endpoint.
    /// </summary>
    public const string CHANGE_EMAIL_VERIFY_OTP = "change-email/verify-otp";

    /// <summary>
    /// Route segment for the current-user KYC submission endpoint.
    /// </summary>
    public const string KYC = "kyc";

    /// <summary>
    /// Route segment for the party-context switch endpoint.
    /// </summary>
    public const string SWITCH_PARTY = "switch-party";

    /// <summary>
    /// Route segment for the register endpoint.
    /// </summary>
    public const string REGISTER = "register";

    /// <summary>
    /// Route segment for the register email-verification endpoint.
    /// </summary>
    public const string VERIFY_EMAIL = "verify-email";
}
