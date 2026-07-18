namespace Be.Haven.ApiCommon.Constants;

/// <summary>
/// Defines reusable example values used by shared Swagger response-envelope examples.
/// </summary>
public static class SwaggerExampleConstants
{
    /// <summary>
    /// Example request identifier shown in Swagger response metadata.
    /// </summary>
    public const string EXAMPLE_REQUEST_ID = "req_01HX7H8K2A4YZ9S8R6P3";

    /// <summary>
    /// Example correlation identifier shown in Swagger response metadata.
    /// </summary>
    public const string EXAMPLE_CORRELATION_ID = "corr_01HX7H8K2A4YZ9S8R6P4";

    /// <summary>
    /// Example distributed-trace identifier shown in Swagger response metadata.
    /// </summary>
    public const string EXAMPLE_TRACE_ID = "4bf92f3577b34da6a3ce929d0e0e4736";

    /// <summary>
    /// Example span identifier shown in Swagger response metadata.
    /// </summary>
    public const string EXAMPLE_SPAN_ID = "00f067aa0ba902b7";

    /// <summary>
    /// Example API version shown in Swagger response metadata.
    /// </summary>
    public const string EXAMPLE_VERSION = "1.0";

    /// <summary>
    /// Example validation field name for an email validation failure.
    /// </summary>
    public const string VALIDATION_FIELD_EMAIL = "email";

    /// <summary>
    /// Example validation field name for a password validation failure.
    /// </summary>
    public const string VALIDATION_FIELD_PASSWORD = "password";

    /// <summary>
    /// Example validation issue for a missing email field.
    /// </summary>
    public const string VALIDATION_EMAIL_ISSUE = "Email is required.";

    /// <summary>
    /// Example validation issue for an invalid password field.
    /// </summary>
    public const string VALIDATION_PASSWORD_ISSUE = "Password must meet the configured policy.";

    /// <summary>
    /// Generic bad-request message used by non-validation Swagger examples.
    /// </summary>
    public const string BAD_REQUEST_MESSAGE = "The request could not be processed.";

}
