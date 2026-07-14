namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Stores authentication infrastructure messages and templates grouped by technical owner.
/// </summary>
public static class InfrastructureMessageConstants
{
    /// <summary>
    /// Contains email subjects and HTML templates used by infrastructure email delivery.
    /// </summary>
    public static class EmailMessages
    {
        /// <summary>
        /// Email subject used when sending register verification OTP emails.
        /// </summary>
        public const string EMAIL_SUBJECT_VERIFY_ACCOUNT = "Verify your Haven account";

        /// <summary>
        /// Email subject used when sending forgot-password OTP emails.
        /// </summary>
        public const string EMAIL_SUBJECT_RESET_PASSWORD = "Reset your Haven password";

        /// <summary>
        /// HTML email template used for OTP delivery.
        /// </summary>
        public const string OTP_EMAIL_HTML_TEMPLATE = "<p>Your Haven OTP is <strong>{0}</strong>. It expires in 5 minutes.</p>";

        /// <summary>
        /// HTML email template used for old-email security notification during change-email flow.
        /// </summary>
        public const string CHANGE_EMAIL_SECURITY_EMAIL_HTML_TEMPLATE = "<p>A request was made to change your Haven account email from <strong>{0}</strong> to <strong>{1}</strong>.</p><p>If this was not you, contact Haven support immediately at <strong>{2}</strong>.</p>";
    }
}
